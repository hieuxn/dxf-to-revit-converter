using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

namespace DXFImporter
{
	public class ModelLineConversionEventHandler : IExternalEventHandler
	{
		private readonly List<ModelLine> _modelLines;
		private readonly FamilySymbolDuplicator _duplicator;
		public string GetName() => "Model Line Conversion";

		public ModelLineConversionEventHandler(List<ModelLine> modelLines, FamilySymbol familySymbol)
		{
			_modelLines = modelLines;
			_duplicator = new FamilySymbolDuplicator(familySymbol.Document);
		}

		public void Execute(UIApplication app)
		{
			try
			{
				var deleteIds = new List<ElementId>();
				var document = app.ActiveUIDocument.Document;
				var level = document.GetElement(document.ActiveView.LevelId) as Level ?? document.ActiveView.GenLevel;
				using var tx = new Transaction(document, "Convert Model Line");
				tx.Start();

				foreach (var modelLine in _modelLines)
				{
					if (false == modelLine.IsValidObject) continue;
					if (modelLine.Location is not LocationCurve locationCurve) continue;
					if (_duplicator.FindOrDuplicate(modelLine) is not { } symbol) return;

					_ = document.Create.NewFamilyInstance(locationCurve.Curve, symbol, level, StructuralType.NonStructural);
					deleteIds.Add(modelLine.Id);
				}

				if (deleteIds.Count > 0)
				{
					document.Delete(deleteIds);
				}

				tx.Commit();
			}
			catch (Exception ex)
			{
				var msg = ex.Message;
				TaskDialog.Show("Error", msg, TaskDialogCommonButtons.Close, TaskDialogResult.Close);
			}
		}
	}
}