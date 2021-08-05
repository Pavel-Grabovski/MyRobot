using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ReinforcementStructuresModel;
using StructuralStrengthening;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace ReinforcementSquareColumns
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class ReinforcementSquareColumnsPresenter
    {

        private ExternalCommandData _revit;
        private ReinforcementSquareColumnsView _view;
        private Document doc;
        private ReinforcementStructures _model;
        private List<Element> SelectedElements = new List<Element>();
        public ReinforcementSquareColumnsPresenter(ExternalCommandData revit, ReinforcementSquareColumnsView view, ReinforcementStructures model)
        {
            this._revit = revit;
            this._view = view;
            this._model = model;

            this.doc = _revit.Application.ActiveUIDocument.Document;

            this._view.AddTypesRebarToComboBoxes(model.RebarTypes);

            _handlerSelectColumnsInadvance();

            this._view.SelectColumnsToProjeckClick += new EventHandler(_view_SelectColumnsButton);
            this._view.SelectColumnToListClick += new EventHandler(_view_SelectColumnComboBox);
            this._view.SeleckAllColumnCheck += new EventHandler(_view_SeleckAllColumnCheck);
            this._view.ReinforceColumnsClick += new EventHandler(_view_ReinforceColumnsButton);
        }

        /// <summary>
        /// обработка элементов, которые были выделены до вызова плагина
        /// </summary>
        private void _handlerSelectColumnsInadvance()
        {
            ICollection<ElementId> elementIdsSelected = _revit.Application.ActiveUIDocument.Selection.GetElementIds();

            foreach (ElementId elementId in elementIdsSelected)
            {
                Element element = doc.GetElement(elementId);
                SelectedElements.Add(element);
            }

            this._view.AddListSelectsColumns(SelectedElements);
            _displayingInfoAboutReceivedObjects(SelectedElements);

        }

        /// <summary>
        /// Обрабатывает начатие "Выбрать все колонны"(checkBox)
        /// </summary>
        private void _view_SeleckAllColumnCheck(object sender, EventArgs e)
        {
            if (_view.GetCheckBoxSeleckAllColumnsIsChecked())
            {
                _view.SetCombBoxColumnsText("");
                _view.SetCombBoxColumnsEnabled(false);
                _view.SetCountColumnValue(SelectedElements.Count);

            }
            else
            {
                _view.SetCombBoxColumnsEnabled(true);
            }
            _displayingInfoAboutReceivedObjects(SelectedElements);
        }

        /// <summary>
        /// Обрабатывает выбор 1 элемента из списка(comboBox)
        /// </summary>
        private void _view_SelectColumnComboBox(object sender, EventArgs e)
        {
            _displayingInfoAboutReceivedObjects(doc.GetElement(new ElementId(_view.ElementIdInt())));
        }

        /// <summary>
        /// Обрабатывает нажатие на кнопку "Выбрать колонны"
        /// </summary>
        private void _view_SelectColumnsButton(object sender, EventArgs e)
        {
            this.SelectedElements.Clear();
            this._view.OffControlsSelectionColumns();
            List<Reference> selectElements = _revit.Application.ActiveUIDocument.Selection.PickObjects(ObjectType.Element).ToList();
            foreach (Reference reference in selectElements)
            {
                Element element = doc.GetElement(reference);
                SelectedElements.Add(element);
            }
            //MessageBox.Show(string.Format($"Выбрано {selectElements.Count} объектов."));

            this._view.AddListSelectsColumns(SelectedElements);
            this._view.OnControlsSelectionColumns();
            _displayingInfoAboutReceivedObjects(SelectedElements);

            this._view.Activate();
        }

        /// <summary>
        /// // Заполнение данных о объектах. Данный метод обрабатывает список
        /// </summary>
        private void _displayingInfoAboutReceivedObjects(List<Element> selectElements)
        {
            if (SelectedElements.Count == 1)
            {
                _displayingInfoAboutReceivedObjects(SelectedElements[0]);
                _view.SetIdValue(SelectedElements[0].Id.ToString());

            }
            if (SelectedElements.Count > 1)
            {
                _view.SetIdValue("<разные>");

                List<string> listFamilyNames = new List<string>();
                foreach (Element element in selectElements)
                {
                    string familyName = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString();
                    listFamilyNames.Add(familyName);
                }
                _isDifferentValuesListAndSetValueLabel(listFamilyNames, _view.SetFamilyValue);

                List<string> listFamilyTypesNames = new List<string>();
                foreach (Element element in selectElements)
                {
                    string familyTypeName = element.Name;
                    listFamilyTypesNames.Add(familyTypeName);
                }
                _isDifferentValuesListAndSetValueLabel(listFamilyTypesNames, _view.SetFamilyTypeValue);

                List<string> listMaterialNames = new List<string>();
                Element elemType;
                foreach (Element element in selectElements)
                {
                    elemType = doc.GetElement(element.GetTypeId());
                    string familyMaterialName = elemType.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).AsValueString();
                    listMaterialNames.Add(familyMaterialName);
                }
                _isDifferentValuesListAndSetValueLabel(listMaterialNames, _view.SetMaterialNameValue);

                /* проверять сечения элементов не имеет смысла, 
                 * т.к. элементы разных сечений в принципе не должны попадать в список выбранных элементов,
                 * потому, что _view_SelectColumnsButton должен выдавать ошибку, если пользователь выбирает элементы с разным сечением.
                 * Поэтому в значения данного параметра будет назначено значение первого элемента из selectElements*/
                elemType = doc.GetElement(selectElements[0].GetTypeId());
                Parameter parameterColumnHeight = elemType.get_Parameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_HEIGHT);
                double columnHeight = this._model.GetValueParameterTypeDouble(parameterColumnHeight);

                Parameter parameterColumnWidth = elemType.get_Parameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_WIDTH);
                double columnWidth = this._model.GetValueParameterTypeDouble(parameterColumnWidth);
                this._view.SetSectionValue($"{columnHeight}{" х "}{columnWidth}{"мм"}");

                /* Та же ситуация с защитными слоями. 
                 * Они должны проверяться при выборке колонн для армирования
                 */
                Parameter parameterRebarCorver = _model.GetParameterRebarCorver(selectElements[0]);
                double ValueParameterRebarCorver = _model.GetValueParameterTypeDouble(parameterRebarCorver);
                this._view.SetRebarCoverTypesValue($"{ValueParameterRebarCorver} mm");
            }
        }

        /// <summary>
        /// Заполнение данных о одном выбранном объекте в спиcке(comboBox)
        /// </summary>
        private void _displayingInfoAboutReceivedObjects(Element element)
        {
            string _familyName = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString();
            this._view.SetFamilyValue(_familyName);
            this._view.SetFamilyTypeValue(element.Name);

            Element elemType = doc.GetElement(element.GetTypeId());
            string elementMaterialName = elemType.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).AsValueString();
            this._view.SetMaterialNameValue(elementMaterialName);

            Parameter parameterColumnHeight = elemType.get_Parameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_HEIGHT);
            double columnHeight = this._model.GetValueParameterTypeDouble(parameterColumnHeight);

            Parameter parameterColumnWidth = elemType.get_Parameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_WIDTH);
            double columnWidth = this._model.GetValueParameterTypeDouble(parameterColumnWidth);

            //Process.Start("cmd", $"/c echo {columnHeight}{" х "}{columnWidth} & pause");
            this._view.SetSectionValue($"{columnHeight}{" х "}{columnWidth}{"мм"}");

            //Значение бокового защитного слоя
            Parameter parameterRebarCorver = _model.GetParameterRebarCorver(element);
            double ValueParameterRebarCorver = _model.GetValueParameterTypeDouble(parameterRebarCorver);
            this._view.SetRebarCoverTypesValue($"{ValueParameterRebarCorver} mm");
        }

        private bool _isDifferentValuesList(List<string> parametersValue)
        {
            if (parametersValue.Distinct().Count() == 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        delegate void _setValueLabel(string value);
        private void _isDifferentValuesListAndSetValueLabel(List<string> parametersValue, _setValueLabel setValueLabel)
        {
            if (parametersValue.Distinct().Count() == 1)
            {
                setValueLabel(parametersValue[0]);
            }
            else
            {
                setValueLabel("<разные>");
            }
        }

        /// <summary>
        /// Вызов внешнего класса для армирования и изменения в модель в немодальном режиме
        /// </summary>
        private void _view_ReinforceColumnsButton(object sender, EventArgs e)
        {

            this._view.ReqHandler.TransferGeneralData(doc, SelectedElements);
            this._view.ReqHandler.TransferForm(_view);
            this._view.ExEvent.Raise();

        }
    }
}
