using System ;
using System.Collections.Generic ;
using System.IO ;
using System.Linq ;
using System.Windows ;
using System.Windows.Forms ;
using Autodesk.Revit.Attributes ;
using Autodesk.Revit.DB ;
using Autodesk.Revit.DB.Events ;
using Autodesk.Revit.DB.Structure ;
using Autodesk.Revit.UI ;
using MessageBox = System.Windows.MessageBox ;
using View = Autodesk.Revit.DB.View ;

namespace DXFImporter
{
  [Transaction( TransactionMode.Manual )]
  public class ActivateCommand : IExternalCommand, IExternalCommandAvailability
  {
    private const string ResourcesPath = @"C:\ProgramData\Autodesk\Revit\Addins\2022\DXFImporter\Resources" ;
    private FamilySymbol? _familySymbol ;
    private ExternalEvent? _event ;

    public bool IsCommandAvailable( UIApplication applicationData, CategorySet selectedCategories )
    {
      return StateManager.Unregister is null ;
    }

    public Result Execute( ExternalCommandData commandData, ref string message, ElementSet elements )
    {
      var document = commandData.Application.ActiveUIDocument.Document ;

      try {
        if ( StateManager.Unregister is null ) {
          if ( _familySymbol is null ) {
            using var tx = new Transaction( document, "Import Family Symbol" ) ;
            tx.Start() ;

            _familySymbol = LoadSymbol( document ).FirstOrDefault() ;
            if ( _familySymbol is null ) return Result.Failed ;
            if ( false == _familySymbol.IsActive ) _familySymbol.Activate() ;

            tx.Commit() ;
          }

          StateManager.Unregister = Unregister ;
          commandData.Application.Application.DocumentChanged += OnDocumentChanged ;
          commandData.Application.Application.DocumentClosed += OnApplicationOnDocumentClosed ;
        }
      }
      catch ( Exception ex ) {
        MessageBox.Show( ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning ) ;
        return Result.Failed ;
      }

      return Result.Succeeded ;
    }

    private static IEnumerable<FamilySymbol> LoadSymbol( Document document )
    {
      var fileNames = Directory.GetFiles( ResourcesPath, $"*.rfa" ) ;
      foreach ( var fileName in fileNames ) {
        var familyName = Path.GetFileName( fileName ).Split( '.' ).First() ;
        if ( document.LoadFamily( fileName, out var family ) ) {
          foreach ( var symbol in family.GetFamilySymbolIds().Select( document.GetElement ).OfType<FamilySymbol>() ) yield return symbol ;
          yield break ;
        }

        yield return new FilteredElementCollector( document )
          .OfCategory( BuiltInCategory.OST_GenericModel )
          .OfClass( typeof( FamilySymbol ) )
          .OfType<FamilySymbol>()
          .FirstOrDefault( symbol => symbol.Family.Name == familyName ) ?? throw new InvalidOperationException() ;
      }
    }

    private void Unregister( Autodesk.Revit.ApplicationServices.Application application )
    {
      application.DocumentChanged -= OnDocumentChanged ;
    }

    private void OnDocumentChanged( object sender, DocumentChangedEventArgs args )
    {
      var doc = args.GetDocument() ;
      var ids = args.GetAddedElementIds() ;
      if ( ids.Select( doc.GetElement ).OfType<ModelLine>().ToList() is not { Count: > 0 } modelLines ) return ;

      var eventHandler = new ModelLineConversionEventHandler( modelLines, _familySymbol! ) ;
      _event = ExternalEvent.Create( eventHandler ) ;
      _event.Raise() ;
    }

    private void OnApplicationOnDocumentClosed( object sender, DocumentClosedEventArgs _ )
    {
      if ( sender is not Autodesk.Revit.ApplicationServices.Application application ) return ;
      Unregister( application ) ;
      application.DocumentClosed -= OnApplicationOnDocumentClosed ;
    }
  }
}