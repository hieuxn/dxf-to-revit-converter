using System ;

namespace DXFImporter
{
  public static class StateManager
  {
    public static Action<Autodesk.Revit.ApplicationServices.Application>? Unregister ;
  }
}