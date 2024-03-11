using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DXFImporter
{
	public class FamilySymbolDuplicator
	{
		private static readonly Dictionary<string, FamilySymbol> Symbols = new();
		private readonly Dictionary<string, FamilySymbol> _dictionary;
		private readonly Document _document;
		private readonly FamilySymbol _proptoTypeSymbol;
		public FamilySymbolDuplicator(Document document)
		{
			_document = document;
			var collector = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol)).OfType<FamilySymbol>();
			collector = collector.Where(fs => fs.Family.FamilyPlacementType == FamilyPlacementType.CurveBased);
			_dictionary = collector.ToDictionary(fs => fs.Name, fs => fs);
			_proptoTypeSymbol = _dictionary.Values.FirstOrDefault();
		}

		public FamilySymbol? FindOrDuplicate(ModelLine modelLine)
		{
			var familyName = modelLine.get_Parameter(BuiltInParameter.BUILDING_CURVE_GSTYLE)?.AsValueString() ?? "0";

			if (false != string.IsNullOrEmpty(familyName)) return null;

			if (false == Symbols.TryGetValue(familyName, out var familySymol) || false == familySymol.IsValidObject)
			{
				familySymol = Find(familyName) ?? (_proptoTypeSymbol.Duplicate(familyName) as FamilySymbol)!;
				Symbols[familyName] = familySymol;
				familySymol.Activate();
			}

			return familySymol;

		}
		public FamilySymbol? Find(string symbolName)
		{
			return _dictionary.TryGetValue(symbolName, out var symbol) ? symbol : null;
		}
	}
}
