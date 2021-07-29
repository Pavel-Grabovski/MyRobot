using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReinforcementStructuresModel
{
    //[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class ReinforcementStructures
    {
        private Document _doc;
        public List<RebarBarType> RebarTypes;

        public ReinforcementStructures(ExternalCommandData revit)
        {
            _doc = revit.Application.ActiveUIDocument.Document;
            var _filter = new ElementIsElementTypeFilter();
            RebarTypes = new FilteredElementCollector(_doc).OfClass(typeof(RebarBarType)).Cast<RebarBarType>().ToList();
        }

        public string GetParameterValueString(Element element, Parameter parameter)
        {
            return parameter.AsValueString();
        }

        public ElementId GetParameterValueElementId(Element element, Parameter parameter)
        {
            return parameter.AsElementId();
        }
        public string GetValueMaterialName(Element element)
        {
            Parameter parameter = element.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
            return parameter.AsValueString();
        }

        /// <summary>
        /// Извлекает численное значение параметра
        /// </summary>
        /// <returns>
        /// Возращает дробное число в милиметрах
        /// </returns>
        public double GetValueParameterTypeDouble(Parameter parameter)
        {
            double value = parameter.AsDouble();

            DisplayUnitType displayUnitType = parameter.DisplayUnitType;
            displayUnitType = DisplayUnitType.DUT_MILLIMETERS;

            value = UnitUtils.ConvertFromInternalUnits(value, displayUnitType);
            return value;
        }


        /// <summary>
        /// Выбирает типы форм арматуры в проекте и возращает указаный тип арматуры по имени.
        /// </summary>
        /// <returns>
        /// Форма арматуры указанной арматуры
        /// </returns>
        /// <exception cref= "System.NullReferenceException">: Ссылка на объект не указывает на экземпляр объекта.</exception>
        public RebarShape GetRebarShape(string name)
        {
            List<RebarShape> rebarShapeList = new FilteredElementCollector(_doc)
                .OfClass(typeof(RebarShape)).Cast<RebarShape>()
                .ToList();

            foreach(RebarShape rebarShape in rebarShapeList)
            {
                if(rebarShape.Name == name)
                {
                    return rebarShape;
                }
            }
            return null;
        }

        /// <summary>
        /// Принимает элементы Revit и возращает параметр защитного слоя "Другие грани"
        /// </summary>
        /// <returns>
        /// Параметр защитного слоя "Другие грани"
        /// </returns>
        public Parameter GetParameterRebarCorver(Element element)
        {
            Parameter parameterRebarCorver = element.get_Parameter(BuiltInParameter.CLEAR_COVER_OTHER);
            Element rebarCorverType = _doc.GetElement(parameterRebarCorver.AsElementId());

            Parameter lenghtRebarCorver = rebarCorverType.get_Parameter(BuiltInParameter.COVER_TYPE_LENGTH);
            return lenghtRebarCorver;
        }

    }
}
