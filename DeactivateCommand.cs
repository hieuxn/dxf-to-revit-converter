using Autodesk.Revit.Attributes ;
using Autodesk.Revit.DB ;
using Autodesk.Revit.UI ;

namespace DXFImporter
{
  [Transaction( TransactionMode.Manual )]
  public class DeactivateCommand : IExternalCommand, IExternalCommandAvailability
  {
    public bool IsCommandAvailable( UIApplication applicationData, CategorySet selectedCategories )
    {
      return StateManager.Unregister is not null ;
    }

    public Result Execute( ExternalCommandData commandData, ref string message, ElementSet elements )
    {
      if ( StateManager.Unregister is not null ) {
        StateManager.Unregister( commandData.Application.Application ) ;
        StateManager.Unregister = null ;
      }

      return Result.Succeeded ;
    }
  }
}