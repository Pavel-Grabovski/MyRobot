using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using ReinforcementSquareColumns.RequestHandler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StructuralStrengthening
{
    public partial class ReinforcementSquareColumnsView : Form
    {
        public event EventHandler SelectColumnsToProjeckClick;
        public event EventHandler SelectColumnToListClick;
        public event EventHandler SeleckAllColumnCheck;
        public event EventHandler ReinforceColumnsClick;

        public RequestHandlerCreateRebar ReqHandler;
        public ExternalEvent ExEvent;

        public ReinforcementSquareColumnsView(ExternalEvent exEvent, RequestHandlerCreateRebar handler)
        {
            InitializeComponent();
            //_clearTextGroupBox(groupParamTypeReinforcement);
            ReqHandler = handler;
            ExEvent = exEvent;
        }

        private void _clearTextGroupBox(GroupBox groupBox)
        {
            /*Сам метод необходим для очистки значений свойства "Text" у Controls элементов
             само стандартное значение данного свойства необходимо, чтобы не путать контролы при работе с формой*/
            foreach(Control cnt in groupBox.Controls)
            {
                cnt.Text = "";
            }
        }

        #region Обработка событий кликов выбора типов армирования
        private void radioButReinforcementType1_Click(object sender, EventArgs e)
        {
            this.groupParamTypeReinforcement.BackgroundImage =
                global::StructuralStrengthening.Properties.Resources.formSquareColumnsReinforcementType1_Section;
            this.groupParamSizeStepReinforcement.BackgroundImage =
                global::StructuralStrengthening.Properties.Resources.formSquareColumnsReinforcementType1_SizeAndStep;

            //groupParamTypeReinforcement Location
            this.comboBoxBasicStirrupBarTapes.Location = new System.Drawing.Point(355, 228);
            this.lblRebarCoverTypes.Location = new System.Drawing.Point(108, 282);
            this.comboBoxFirstMainBarTapes.Location = new System.Drawing.Point(25, 65);

            //groupParamTypeReinforcement Remove
            this.groupParamTypeReinforcement.Controls.Remove(this.comboBoxSecondStirrupBarTapes);
            this.groupParamTypeReinforcement.Controls.Remove(this.comboBoxSecondMainBarTapes);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterLeftRebarOffset_1);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterTopRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterTopRebarOffset_1);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterLowerRebarOffset_1);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterLowerRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterLeftRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterRightRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterRightRebarOffset_1);


            // groupParamSizeStepReinforcement Location
            this.textBoxRebarOutletsLength.Location = new System.Drawing.Point(230, 83);
            this.textBoxCountMiddleTransverseReinforcement.Location = new System.Drawing.Point(236, 321);
            this.textBoxCountLowerTransverseReinforcement.Location = new System.Drawing.Point(236, 445);
            this.textBoxFirstStirrupOffset.Location = new System.Drawing.Point(65, 476);
            this.textBoxStepLowerTransverseReinforcement.Location = new System.Drawing.Point(59, 445);
            this.textBoxStepMiddleTransverseReinforcement.Location = new System.Drawing.Point(62, 324);
            this.textBoxStepTopTransverseReinforcement.Location = new System.Drawing.Point(59, 191);
            this.textBoxOverlapThickness.Location = new System.Drawing.Point(18, 136);
        }
        private void radioButReinforcementType2_Click(object sender, EventArgs e)
        {
            this.groupParamTypeReinforcement.BackgroundImage =
                global::StructuralStrengthening.Properties.Resources.formSquareColumnsReinforcementType2_Section;
            this.groupParamSizeStepReinforcement.BackgroundImage =
                global::StructuralStrengthening.Properties.Resources.formSquareColumnsReinforcementType2_SizeAndStep;

            this.groupParamTypeReinforcement.Controls.Add(this.comboBoxSecondMainBarTapes);

            //groupParamTypeReinforcement Location
            this.comboBoxBasicStirrupBarTapes.Location = new System.Drawing.Point(370, 112);
            this.comboBoxSecondMainBarTapes.Location = new System.Drawing.Point(369, 277);
            this.lblRebarCoverTypes.Location = new System.Drawing.Point(108, 282);
            this.comboBoxFirstMainBarTapes.Location = new System.Drawing.Point(15, 35);

            //groupParamTypeReinforcement Remove
            this.groupParamTypeReinforcement.Controls.Remove(this.comboBoxSecondStirrupBarTapes);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterLeftRebarOffset_1);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterTopRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterTopRebarOffset_1);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterLowerRebarOffset_1);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterLowerRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterLeftRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterRightRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterRightRebarOffset_1);


            // groupParamSizeStepReinforcement Location
            this.textBoxRebarOutletsLength.Location = new System.Drawing.Point(230, 83);
            this.textBoxCountMiddleTransverseReinforcement.Location = new System.Drawing.Point(236, 321);
            this.textBoxCountLowerTransverseReinforcement.Location = new System.Drawing.Point(236, 445);
            this.textBoxFirstStirrupOffset.Location = new System.Drawing.Point(65, 476);
            this.textBoxStepLowerTransverseReinforcement.Location = new System.Drawing.Point(59, 445);
            this.textBoxStepMiddleTransverseReinforcement.Location = new System.Drawing.Point(62, 324);
            this.textBoxStepTopTransverseReinforcement.Location = new System.Drawing.Point(59, 191);
            this.textBoxOverlapThickness.Location = new System.Drawing.Point(18, 136);
        }
        private void radioButReinforcementType3_Click(object sender, EventArgs e)
        {
            this.groupParamTypeReinforcement.BackgroundImage =
                global::StructuralStrengthening.Properties.Resources.formSquareColumnsReinforcementType3_Section;
            this.groupParamSizeStepReinforcement.BackgroundImage =
                global::StructuralStrengthening.Properties.Resources.formSquareColumnsReinforcementType3_SizeAndStep;

            //groupParamTypeReinforcement Add
            this.groupParamTypeReinforcement.Controls.Add(this.comboBoxSecondStirrupBarTapes);
            this.groupParamTypeReinforcement.Controls.Add(this.comboBoxSecondMainBarTapes);

            //groupParamTypeReinforcement Location
            this.comboBoxBasicStirrupBarTapes.Location = new System.Drawing.Point(370, 52);
            this.comboBoxSecondStirrupBarTapes.Location = new System.Drawing.Point(370, 131);
            this.comboBoxSecondMainBarTapes.Location = new System.Drawing.Point(370, 292);
            this.lblRebarCoverTypes.Location = new System.Drawing.Point(97, 280);
            this.comboBoxFirstMainBarTapes.Location = new System.Drawing.Point(18, 53);

            //groupParamTypeReinforcement Remove
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterTopRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterTopRebarOffset_1);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterLowerRebarOffset_1);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterLowerRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterLeftRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterLeftRebarOffset_1);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterRightRebarOffset_1);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterRightRebarOffset_2);

            // groupParamSizeStepReinforcement Location
            this.textBoxRebarOutletsLength.Location = new System.Drawing.Point(230, 83);
            this.textBoxCountMiddleTransverseReinforcement.Location = new System.Drawing.Point(236, 321);
            this.textBoxCountLowerTransverseReinforcement.Location = new System.Drawing.Point(236, 445);
            this.textBoxFirstStirrupOffset.Location = new System.Drawing.Point(65, 476);
            this.textBoxStepLowerTransverseReinforcement.Location = new System.Drawing.Point(59, 445);
            this.textBoxStepMiddleTransverseReinforcement.Location = new System.Drawing.Point(62, 324);
            this.textBoxStepTopTransverseReinforcement.Location = new System.Drawing.Point(59, 191);
            this.textBoxOverlapThickness.Location = new System.Drawing.Point(18, 136);
        }
        private void radioButReinforcementType4_Click(object sender, EventArgs e)
        {
            this.groupParamTypeReinforcement.BackgroundImage =
                global::StructuralStrengthening.Properties.Resources.formSquareColumnsReinforcementType4_Section;
            this.groupParamSizeStepReinforcement.BackgroundImage =
                global::StructuralStrengthening.Properties.Resources.formSquareColumnsReinforcementType4_SizeAndStep;

            //groupParamTypeReinforcement Add
            this.groupParamTypeReinforcement.Controls.Add(this.comboBoxSecondStirrupBarTapes);
            this.groupParamTypeReinforcement.Controls.Add(this.comboBoxSecondMainBarTapes);
            this.groupParamTypeReinforcement.Controls.Add(this.textBoxSecondСenterTopRebarOffset_1);
            this.groupParamTypeReinforcement.Controls.Add(this.textBoxSecondСenterLowerRebarOffset_1);
            this.groupParamTypeReinforcement.Controls.Add(this.textBoxSecondСenterLeftRebarOffset_1);
            this.groupParamTypeReinforcement.Controls.Add(this.textBoxSecondСenterRightRebarOffset_1);

            //groupParamTypeReinforcement Location 
            this.comboBoxBasicStirrupBarTapes.Location = new System.Drawing.Point(370, 25);
            this.comboBoxSecondStirrupBarTapes.Location = new System.Drawing.Point(370, 104);
            this.comboBoxSecondMainBarTapes.Location = new System.Drawing.Point(370, 264);
            this.textBoxSecondСenterRightRebarOffset_1.Location = new System.Drawing.Point(260, 303);
            this.textBoxSecondСenterLeftRebarOffset_1.Location = new System.Drawing.Point(187, 303);
            this.lblRebarCoverTypes.Location = new System.Drawing.Point(97, 273);
            this.textBoxSecondСenterLowerRebarOffset_1.Location = new System.Drawing.Point(73, 164);
            this.textBoxSecondСenterTopRebarOffset_1.Location = new System.Drawing.Point(73, 138);
            this.comboBoxFirstMainBarTapes.Location = new System.Drawing.Point(20, 25);

            //groupParamTypeReinforcement Remove
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterTopRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterLowerRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterLeftRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterRightRebarOffset_2);


            // groupParamSizeStepReinforcement Location
            this.textBoxRebarOutletsLength.Location = new System.Drawing.Point(230, 83);
            this.textBoxCountMiddleTransverseReinforcement.Location = new System.Drawing.Point(242, 321);
            this.textBoxCountLowerTransverseReinforcement.Location = new System.Drawing.Point(242, 445);
            this.textBoxFirstStirrupOffset.Location = new System.Drawing.Point(67, 476);
            this.textBoxStepLowerTransverseReinforcement.Location = new System.Drawing.Point(55, 444);
            this.textBoxStepMiddleTransverseReinforcement.Location = new System.Drawing.Point(56, 323);
            this.textBoxStepTopTransverseReinforcement.Location = new System.Drawing.Point(56, 191);
            this.textBoxOverlapThickness.Location = new System.Drawing.Point(19, 137);
        }
        private void radioButReinforcementType5_Click(object sender, EventArgs e)
        {
            this.groupParamTypeReinforcement.BackgroundImage =
                global::StructuralStrengthening.Properties.Resources.formSquareColumnsReinforcementType5_Section;
            this.groupParamSizeStepReinforcement.BackgroundImage =
                global::StructuralStrengthening.Properties.Resources.formSquareColumnsReinforcementType5_SizeAndStep;

            //groupParamTypeReinforcement Add
            this.groupParamTypeReinforcement.Controls.Add(this.comboBoxSecondStirrupBarTapes);
            this.groupParamTypeReinforcement.Controls.Add(this.comboBoxSecondMainBarTapes);
            this.groupParamTypeReinforcement.Controls.Add(this.textBoxSecondСenterTopRebarOffset_1);
            this.groupParamTypeReinforcement.Controls.Add(this.textBoxSecondСenterLowerRebarOffset_1);
            this.groupParamTypeReinforcement.Controls.Add(this.textBoxSecondСenterLeftRebarOffset_1);
            this.groupParamTypeReinforcement.Controls.Add(this.textBoxSecondСenterRightRebarOffset_1);

            //groupParamTypeReinforcement Location 
            this.comboBoxBasicStirrupBarTapes.Location = new System.Drawing.Point(370, 25);
            this.comboBoxSecondStirrupBarTapes.Location = new System.Drawing.Point(370, 103);
            this.comboBoxSecondMainBarTapes.Location = new System.Drawing.Point(370, 263);
            this.textBoxSecondСenterRightRebarOffset_1.Location = new System.Drawing.Point(265, 303);
            this.textBoxSecondСenterLeftRebarOffset_1.Location = new System.Drawing.Point(183, 303);
            this.lblRebarCoverTypes.Location = new System.Drawing.Point(97, 269);
            this.textBoxSecondСenterLowerRebarOffset_1.Location = new System.Drawing.Point(73, 168);
            this.textBoxSecondСenterTopRebarOffset_1.Location = new System.Drawing.Point(73, 134);
            this.comboBoxFirstMainBarTapes.Location = new System.Drawing.Point(20, 25);

            //groupParamTypeReinforcement Remove
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterTopRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterLowerRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterLeftRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Remove(this.textBoxSecondСenterRightRebarOffset_2);


            // groupParamSizeStepReinforcement Location
            this.textBoxRebarOutletsLength.Location = new System.Drawing.Point(249, 83);
            this.textBoxCountMiddleTransverseReinforcement.Location = new System.Drawing.Point(248, 321);
            this.textBoxCountLowerTransverseReinforcement.Location = new System.Drawing.Point(249, 445);
            this.textBoxFirstStirrupOffset.Location = new System.Drawing.Point(46, 477);
            this.textBoxStepLowerTransverseReinforcement.Location = new System.Drawing.Point(46, 444);
            this.textBoxStepMiddleTransverseReinforcement.Location = new System.Drawing.Point(48, 323);
            this.textBoxStepTopTransverseReinforcement.Location = new System.Drawing.Point(45, 190);
            this.textBoxOverlapThickness.Location = new System.Drawing.Point(15, 136);
        }
        private void radioButReinforcementType6_Click(object sender, EventArgs e)
        {
            this.groupParamTypeReinforcement.BackgroundImage =
                global::StructuralStrengthening.Properties.Resources.formSquareColumnsReinforcementType6_Section;
            this.groupParamSizeStepReinforcement.BackgroundImage =
                global::StructuralStrengthening.Properties.Resources.formSquareColumnsReinforcementType6_SizeAndStep;

            //groupParamTypeReinforcement Add
            this.groupParamTypeReinforcement.Controls.Add(this.comboBoxSecondStirrupBarTapes);
            this.groupParamTypeReinforcement.Controls.Add(this.comboBoxSecondMainBarTapes);
            this.groupParamTypeReinforcement.Controls.Add(this.textBoxSecondСenterLeftRebarOffset_1);
            this.groupParamTypeReinforcement.Controls.Add(this.textBoxSecondСenterTopRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Add(this.textBoxSecondСenterTopRebarOffset_1);
            this.groupParamTypeReinforcement.Controls.Add(this.textBoxSecondСenterLowerRebarOffset_1);
            this.groupParamTypeReinforcement.Controls.Add(this.textBoxSecondСenterLowerRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Add(this.textBoxSecondСenterLeftRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Add(this.textBoxSecondСenterRightRebarOffset_2);
            this.groupParamTypeReinforcement.Controls.Add(this.textBoxSecondСenterRightRebarOffset_1);

            //groupParamTypeReinforcement Location
            this.comboBoxBasicStirrupBarTapes.Location = new System.Drawing.Point(375, 16);
            this.comboBoxSecondStirrupBarTapes.Location = new System.Drawing.Point(377, 110);
            this.comboBoxSecondMainBarTapes.Location = new System.Drawing.Point(375, 269);
            this.textBoxSecondСenterRightRebarOffset_2.Location = new System.Drawing.Point(317, 306);
            this.textBoxSecondСenterRightRebarOffset_1.Location = new System.Drawing.Point(256, 310);
            this.textBoxSecondСenterLeftRebarOffset_1.Location = new System.Drawing.Point(192, 310);
            this.textBoxSecondСenterLeftRebarOffset_2.Location = new System.Drawing.Point(120, 305);
            this.lblRebarCoverTypes.Location = new System.Drawing.Point(84, 257);
            this.textBoxSecondСenterLowerRebarOffset_2.Location = new System.Drawing.Point(37, 188);
            this.textBoxSecondСenterLowerRebarOffset_1.Location = new System.Drawing.Point(37, 162);
            this.textBoxSecondСenterTopRebarOffset_1.Location = new System.Drawing.Point(37, 137);
            this.textBoxSecondСenterTopRebarOffset_2.Location = new System.Drawing.Point(37, 104);
            this.comboBoxFirstMainBarTapes.Location = new System.Drawing.Point(5, 14);


            // groupParamSizeStepReinforcement Location
            this.textBoxRebarOutletsLength.Location = new System.Drawing.Point(249, 83);
            this.textBoxCountMiddleTransverseReinforcement.Location = new System.Drawing.Point(248, 321);
            this.textBoxCountLowerTransverseReinforcement.Location = new System.Drawing.Point(249, 445);
            this.textBoxFirstStirrupOffset.Location = new System.Drawing.Point(46, 477);
            this.textBoxStepLowerTransverseReinforcement.Location = new System.Drawing.Point(46, 444);
            this.textBoxStepMiddleTransverseReinforcement.Location = new System.Drawing.Point(48, 323);
            this.textBoxStepTopTransverseReinforcement.Location = new System.Drawing.Point(45, 190);
            this.textBoxOverlapThickness.Location = new System.Drawing.Point(15, 136);
        }

        #endregion

        private void checkBoxSeleckAllColumns_Click(object sender, EventArgs e)
        {
            if (SeleckAllColumnCheck != null)
            {
                SeleckAllColumnCheck(this, EventArgs.Empty);
            }
        }
        public void AddTypesRebarToComboBoxes(List<RebarBarType> rebarTypes)
        {
            foreach (RebarBarType rebarType in rebarTypes)
            {
                this.comboBoxFirstMainBarTapes.Items.Add(rebarType);
                this.comboBoxFirstMainBarTapes.DisplayMember = "Name";
                this.comboBoxBasicStirrupBarTapes.Items.Add(rebarType);
                this.comboBoxBasicStirrupBarTapes.DisplayMember = "Name";
                this.comboBoxSecondStirrupBarTapes.Items.Add(rebarType);
                this.comboBoxSecondStirrupBarTapes.DisplayMember = "Name";
                this.comboBoxSecondMainBarTapes.Items.Add(rebarType);
                this.comboBoxSecondMainBarTapes.DisplayMember = "Name";
            }
        }

        private void butSelectColumns_Click(object sender, EventArgs e)
        {
            if(SelectColumnsToProjeckClick != null)
            {
                SelectColumnsToProjeckClick(this, EventArgs.Empty);
            }
        }

        public void AddListSelectsColumns(List<Autodesk.Revit.DB.Element> selectElements)
        {
            this.combBoxColumns.Items.Clear();
            SetCountColumnValue(selectElements.Count);
            foreach (Autodesk.Revit.DB.Element element in selectElements)
            {
                this.combBoxColumns.Items.Add(element);
                this.combBoxColumns.DisplayMember = "Id";
            }
        }
        /*public void AddListSelectsColumns(List<Autodesk.Revit.DB.ElementId> selectElements)
        {
            this.combBoxColumns.Items.Clear();
            SetCountColumnValue(selectElements.Count);
            foreach (Autodesk.Revit.DB.ElementId elementId in selectElements)
            {
                this.combBoxColumns.Items.Add(elementId);
                this.combBoxColumns.DisplayMember = "Id";
            }
        }*/

        private void _comboBoxColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListControl listControl = (ListControl)sender;
            this.lblIdValue.Text = listControl.Text;
            SetCountColumnValue(1);


            if (SelectColumnToListClick != null)
            {
                SelectColumnToListClick(this, EventArgs.Empty);
            }
        }

        public int ElementIdInt()
        {
            return Convert.ToInt32(this.combBoxColumns.Text);
        }
        public void SetFamilyValue(string value)
        {
            this.lblFamilyValue.Text = value;
        }
        public void SetFamilyTypeValue(string value)
        {
            this.lblFamilyTypeslValue.Text = value;
        }
        public void SetMaterialNameValue(string value)
        {
            this.lblMaterialValue.Text = value;
        }
        public void SetSectionValue(string value)
        {
            this.lblSectionValue.Text = value;
        }

        /// <summary>
        /// Отключение контролов во время выбора колонн в проекте
        /// </summary>
        public void OffControlsSelectionColumns()
        {
            foreach (Control control in groupBoxSelectionColumns.Controls)
            {
                if(control.Name != "combBoxColumns")
                {
                    control.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Активация контролов после выбора колонн в проекте
        /// </summary>
        public void OnControlsSelectionColumns()
        {
            foreach (Control control in groupBoxSelectionColumns.Controls)
            {
                if (control.Name != "combBoxColumns")
                {
                    control.Enabled = true;
                }
            }
        }

        public bool GetCheckBoxSeleckAllColumnsIsChecked()
        {
            return checkBoxSeleckAllColumns.Checked;
        }
        public void SetCombBoxColumnsText(string value)
        {
            combBoxColumns.Text = "";
        }
        public void SetCombBoxColumnsEnabled(bool boolean)
        {
            combBoxColumns.Enabled = boolean;
        }
        public void SetCountColumnValue(int value)
        {
            this.lblCountColumnValue.Text = Convert.ToString(value);
        }
        public void SetIdValue(string value)
        {
            this.lblIdValue.Text = value;
        }

        private void butReinforceColumns_Click(object sender, EventArgs e)
        {
            if (ReinforceColumnsClick != null)
            {
                ReinforceColumnsClick(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Присвоить значение значение защитного слоя
        /// </summary>
        public void SetRebarCoverTypesValue(string value)
        {
            this.lblRebarCoverTypes.Text = value;
        }

        /// <summary>
        /// Активный выбранный типоразмер основной продольной арматуры
        /// </summary>
        /// <returns>
        /// Типоразмер, выбранной пользователем, основной продольной арматуры.
        /// </returns>
        /// <exception cref= "System.NullReferenceException">: Ссылка на объект не указывает на экземпляр объекта.</exception>
        public RebarBarType GetFirstMainBarTapes()
        {
            return (RebarBarType)this.comboBoxFirstMainBarTapes.SelectedItem;
        }

        /// <summary>
        /// Активный выбранный типоразмер второстепенной продольной арматуры
        /// </summary>
        /// <returns>
        /// Типоразмер, выбранной пользователем, второстепенной продольной арматуры
        /// </returns>
        /// <exception cref= "System.NullReferenceException">: Ссылка на объект не указывает на экземпляр объекта.</exception>
        public RebarBarType GetSecondMainBarTapes()
        {
            return (RebarBarType)this.comboBoxSecondMainBarTapes.SelectedItem;
        }

        /// <summary>
        /// Получить тип выбранного армирования
        /// </summary>
        /// <returns>
        /// Наименование выбранного RadioButton в группе типов армирования
        /// </returns>
        public string GetActiveReinforcementType()
        {
            foreach(Control control in this.groupBoxReinforcementType.Controls)
            {
                if(control.GetType().Name == "RadioButton")
                {
                    RadioButton radioButton = (RadioButton)control;
                    if (radioButton.Checked)
                    {
                        return radioButton.Name;
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Получить тип выпусков арматуры
        /// </summary>
        /// <returns>
        /// Наименование выбранного RadioButton в группе выбора выпусков арматуры
        /// </returns>
        public string GetActiveRebarOutletTypes()
        {
            foreach (Control control in this.groupBoxRebarOutletTypes.Controls)
            {
                if (control.GetType().Name == "RadioButton")
                {
                    RadioButton radioButton = (RadioButton)control;
                    if (radioButton.Checked)
                    {
                        return radioButton.Name;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Обращение к textBox, где вводится значения длины выпусков арматуры
        /// </summary>
        /// <returns>
        /// Длина выпусков арматуры в мм 
        /// </returns>
        public int GetValueRebarOutletsLength()
        {
            return Convert.ToInt32(this.textBoxRebarOutletsLength.Text);
        }

        /// <summary>
        /// Обращение к textBox, где вводится значения толщины плиты перекрытия
        /// </summary>
        /// <returns>
        /// Толщина плиты перекрытия в мм 
        /// </returns>
        public double GetValueOverlapThickness()
        {
            return Convert.ToDouble(this.textBoxOverlapThickness.Text);
        }

        /// <summary>
        /// Обращение к textBox, где вводится значения отступа первого нижнего центрально-левого стержня
        /// </summary>
        /// <returns>
        /// Отступ первого нижнего центрально-левого стержня, указанный пользователем
        /// </returns>
        public double GetValueOffsetSecondСenterLeftRebar_1()
        {
            return Convert.ToDouble(this.textBoxSecondСenterLeftRebarOffset_1.Text);
        }

        /// <summary>
        /// Обращение к textBox, где вводится значения отступа первого нижнего центрально-правого стержня
        /// </summary>
        /// <returns>
        /// Отступ первого нижнего центрально-правого стержня, указанный пользователем
        /// </returns>
        public double GetValueOffsetSecondСenterRightRebar_1()
        {
            return Convert.ToDouble(this.textBoxSecondСenterRightRebarOffset_1.Text);
        }

        /// <summary>
        /// Обращение к textBox, где вводится значения отступа первого правого центрально-верхнего стержня
        /// </summary>
        /// <returns>
        /// Отступ первого центрально-верхнего, указанный пользователем
        /// </returns>
        public double GetValueOffsetSecondСenterTopRebar_1()
        {
            return Convert.ToDouble(this.textBoxSecondСenterTopRebarOffset_1.Text);
        }

        /// <summary>
        /// Обращение к textBox, где вводится значения отступа первого правого центрально-нижнего стержня
        /// </summary>
        /// <returns>
        /// Отступ первого центрально-нижнего, указанный пользователем
        /// </returns>
        public double GetValueOffsetSecondСenterLowerRebar_1()
        {
            return Convert.ToDouble(this.textBoxSecondСenterLowerRebarOffset_1.Text);
        }


        /// <summary>
        /// Обращение к textBox, где вводится значения отступа первого нижнего центрально-левого стержня
        /// </summary>
        /// <returns>
        /// Отступ первого нижнего центрально-левого стержня, указанный пользователем
        /// </returns>
        public double GetValueOffsetSecondСenterLeftRebar_2()
        {
            return Convert.ToDouble(this.textBoxSecondСenterLeftRebarOffset_2.Text);
        }

        /// <summary>
        /// Обращение к textBox, где вводится значения отступа первого нижнего центрально-правого стержня
        /// </summary>
        /// <returns>
        /// Отступ первого нижнего центрально-правого стержня, указанный пользователем
        /// </returns>
        public double GetValueOffsetSecondСenterRightRebar_2()
        {
            return Convert.ToDouble(this.textBoxSecondСenterRightRebarOffset_2.Text);
        }

        /// <summary>
        /// Обращение к textBox, где вводится значения отступа второго правого центрально-верхнего стержня
        /// </summary>
        /// <returns>
        /// Отступ второго центрально-верхнего нижнего, указанный пользователем
        /// </returns>
        public double GetValueOffsetSecondСenterTopRebar_2()
        {
            return Convert.ToDouble(this.textBoxSecondСenterTopRebarOffset_2.Text);
        }

        /// <summary>
        /// Обращение к textBox, где вводится значения отступа второго правого центрально-нижнего стержня
        /// </summary>
        /// <returns>
        /// Отступ второго центрально-нижнего стержня, указанный пользователем
        /// </returns>
        public double GetValueOffsetSecondСenterLowerRebar_2()
        {
            return Convert.ToDouble(this.textBoxSecondСenterLowerRebarOffset_2.Text);
        }

        /// <summary>
        /// Обращение к textBox, где вводится значения отступа снизу от начало колонны всех продольных стержней
        /// </summary>
        /// <returns>
        /// Отступ снизу от начало колонны всех продольных стержней
        /// </returns>
        public double GetValueBoxBottomOffsetMainBars()
        {
            return Convert.ToDouble(this.textBoxBottomOffsetMainBars.Text);
        }

        /// <summary>
        /// Обращение к textBox, где вводится значения дополнительного смещения изгиба стержня
        /// </summary>
        /// <returns>
        /// Дополнительное спещение загиба стержня
        /// </returns>
        public double GetValueAdditionalOffsetBendBar()
        {
            return Convert.ToDouble(this.textBoxAdditionalOffsetBendBar.Text);
        }

        /// <summary>
        /// Покрасить textBox, где вводится значения дополнительного смещения изгиба стержня
        /// </summary>
        public void SetColorTextBoxAdditionalOffsetBendBar(string color)
        {  
            if(color == "Red")
            {
                this.textBoxAdditionalOffsetBendBar.BackColor = Color.Red;
            }
            else if(color == "White")
            {
                this.textBoxAdditionalOffsetBendBar.BackColor = Color.White;
            }
            
        }

        /// <summary>
        /// Обращение к checkBox, нужно ли повернуть выпуски внутрь колонны или нет
        /// </summary>
        public bool GetOutletsInside()
        {
            return this.checkBoxIsOutletsInside.Checked;
        }

        /// <summary>
        /// Обращение к checkBox, показать арматуру на виде 3D телом.
        /// </summary>
        public bool GetSolidInView()
        {
            return this.checkBoxRebarSolidInView.Checked;
        }
        /// <summary>
        /// Обращение к checkBox, показать арматуру неперекрытой
        /// </summary>
        public bool GetShowUnoverlapped()
        {
            return this.checkBoxShowUnoverlapped.Checked;
        }

    }
}

