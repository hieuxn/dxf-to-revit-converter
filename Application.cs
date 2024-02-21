using System ;
using System.Windows ;
using System.Windows.Media.Imaging ;
using Autodesk.Revit.UI ;

namespace DXFImporter
{
  public class Application : IExternalApplication
  {
    private const string ApplicationName = "DXFImporter" ;

    public Result OnStartup( UIControlledApplication application )
    {
      try {
        var ribbonPanel = application.CreateRibbonPanel( ApplicationName ) ;

        var assemblyName = this.GetType().Assembly.Location ;

        var buttonData1 = new PushButtonData( "Activate", "Activate", assemblyName, typeof( ActivateCommand ).FullName )
        {
          AvailabilityClassName = typeof( ActivateCommand ).FullName
        } ;
        var button1 = ribbonPanel.AddItem( buttonData1 ) as PushButton ;
        button1!.LargeImage = new BitmapImage( new Uri( $"pack://application:,,,/{ApplicationName};component/Resources/activate.png" ) ) ;

        var buttonData2 = new PushButtonData( "Deactivate", "Deactivate", assemblyName, typeof( DeactivateCommand ).FullName )
        {
          AvailabilityClassName = typeof( DeactivateCommand ).FullName
        } ;
        var button2 = ribbonPanel.AddItem( buttonData2 ) as PushButton ;
        button2!.LargeImage = new BitmapImage( new Uri( $"pack://application:,,,/{ApplicationName};component/Resources/deactivate.png" ) ) ;

        return Result.Succeeded ;
      }
      catch ( Exception ex ) {
        MessageBox.Show( ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning ) ;
        return Result.Failed ;
      }
    }

    public Result OnShutdown( UIControlledApplication application )
    {
      return Result.Succeeded ;
    }
  }
}