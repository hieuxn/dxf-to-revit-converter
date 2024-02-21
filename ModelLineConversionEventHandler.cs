using System.Collections.Generic ;
using Autodesk.Revit.DB ;
using Autodesk.Revit.DB.Structure ;
using Autodesk.Revit.UI ;

namespace DXFImporter
{
  public class ModelLineConversionEventHandler : IExternalEventHandler
  {
    private readonly List<ModelLine> _modelLines ;
    private readonly FamilySymbol _familySymbol ;
    public string GetName() => "Model Line Conversion" ;

    public ModelLineConversionEventHandler( List<ModelLine> modelLines, FamilySymbol familySymbol )
    {
      _modelLines = modelLines ;
      _familySymbol = familySymbol ;
    }

    public void Execute( UIApplication app )
    {
      var deleteIds = new List<ElementId>() ;
      var document = app.ActiveUIDocument.Document ;
      var level = document.GetElement( document.ActiveView.LevelId ) as Level ?? document.ActiveView.GenLevel ;
      using var tx = new Transaction( document, "Convert Model Line" ) ;
      tx.Start() ;

      foreach ( var modelLine in _modelLines ) {
        if ( false == modelLine.IsValidObject ) continue ;
        deleteIds.Add( modelLine.Id ) ;
        if ( modelLine.Location is not LocationCurve locationCurve ) continue ;
        _ = document.Create.NewFamilyInstance( locationCurve.Curve, _familySymbol, level, StructuralType.NonStructural ) ;
      }

      if ( deleteIds.Count > 0 ) {
        document.Delete( deleteIds ) ;
      }

      tx.Commit() ;
    }
  }
}