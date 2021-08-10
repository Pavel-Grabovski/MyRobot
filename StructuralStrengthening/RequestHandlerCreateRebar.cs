
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using StructuralStrengthening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ReinforcementSquareColumns.RequestHandler
{

    public class RequestHandlerCreateRebar : IExternalEventHandler
    {
        private ReinforcementSquareColumnsView _view;

        private List<Element> _selectedElements;
        private Document doc;

        private string _activeRebarOutletTypes;
        private string _activeReinforcementType;

        /// <summary>
        /// Толщина плиты перекрытия
        /// </summary>
        private double _overlapThickness;

        /// <summary>
        /// Длина выпусков арматуры
        /// </summary>
        private double _rebarOutletsLength;

        /// <summary>
        /// Отступ начало построения продольной арматуры снизу от колонны
        /// </summary>
        private double _bottomOffsetCrossBars;

        /// <summary>
        /// Отступ начало построения хомутов снизу колонны
        /// </summary>
        private double _bottomOffsetBasicClamp;

        /// <summary>
        /// Дополнительное смещение выпуска гнутой арматуры
        /// </summary>
        private double _additionalOffsetBendBar;

        /// <summary>
        /// Форма прямой арматуры 
        /// </summary>
        RebarShape rebarSmoothShape;

        /// <summary>
        /// Форма гнутой арматуры 
        /// </summary>
        RebarShape rebarCurvedShape;

        /// <summary>
        /// Форма гнутой арматуры 
        /// </summary>
        RebarShape rebarClamp;

        /// <summary>
        /// Отгибы арматуры
        /// </summary>
        RebarHookType rebarHookType;

        public void TransferGeneralData(Document doc, List<Element> elements)
        {
            this._selectedElements = elements;
            this.doc = doc;

        }

        /// <summary>
        ///Передача данных с пользовательской формы
        /// </summary>
        public void TransferForm(ReinforcementSquareColumnsView view)
        {
            this._view = view;
            this._activeRebarOutletTypes = _view.GetActiveRebarOutletTypes();
            this._activeReinforcementType = _view.GetActiveReinforcementType();

            this._overlapThickness = (_view.GetValueOverlapThickness() / 304.8);

            this._rebarOutletsLength = (_view.GetValueRebarOutletsLength() / 304.8);
            this._bottomOffsetCrossBars = (_view.GetValueBoxBottomOffsetMainBars() / 304.8);
            this._additionalOffsetBendBar = (_view.GetValueAdditionalOffsetBendBar() / 304.8);

            this._bottomOffsetBasicClamp = (_view.GetValueBoxBottomOffsetBasicClamp() / 304.8);

            rebarSmoothShape = _getRebarShape("О_1");
            rebarCurvedShape = _getRebarShape("О_26(α»90)");
            rebarClamp = _getRebarShape("Х_51");

            rebarHookType = _getRebarHookType("Хомут/стяжка_135°");
        }


        public void Execute(UIApplication app)
        {
            using (Transaction t = new Transaction(doc))
            {

                t.Start("Армирование квадратной колонны");
                int totalMessage = 0; // Чтобы сообщения не повторялись по нескольку раз во время работы цикла

                foreach (Element element in _selectedElements)
                {
                    #region Сбор параметров клонны
                    //Сбор параметров колонны
                    //Отметки уровня
                    Level topLevel = (Level)doc.GetElement(element.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).AsElementId());
                    double topLevelElevation = Math.Round(topLevel.Elevation, 6);
                    Level baseLevel = (Level)doc.GetElement(element.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM).AsElementId());
                    double baseLevelElevation = Math.Round(baseLevel.Elevation, 6);

                    //Отступы от уровней

                    Parameter topLevelOffsetParam = element.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM);
                    double topLevelOffset = Math.Round(topLevelOffsetParam.AsDouble(), 6);
                    Parameter baseLevelOffsetParam = element.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM);
                    double baseLevelOffset = Math.Round(baseLevelOffsetParam.AsDouble(), 6);
                    //длина колонны
                    double elementLength = ((topLevelElevation + topLevelOffset) - (baseLevelElevation + baseLevelOffset));
                    // Боковой защитный слой колонны
                    Parameter parameterRebarCorver = GetParameterRebarCorver(element);
                    double rebarCorver = parameterRebarCorver.AsDouble();

                    //Получение местоположения (точки ее вставки в проектe)
                    LocationPoint columnOriginLocationPoint = element.Location as LocationPoint;
                    XYZ columnOriginBase = columnOriginLocationPoint.Point;
                    XYZ elementOrigin = new XYZ(columnOriginBase.X, columnOriginBase.Y, baseLevelElevation + baseLevelOffset);
                    //Свойсва сечения колонны
                    Element elemType = doc.GetElement(element.GetTypeId());

                    Parameter parameterColumnHeight = elemType.get_Parameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_HEIGHT);
                    double columnHeight = parameterColumnHeight.AsDouble();
                    Parameter parameterColumnWidth = elemType.get_Parameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_WIDTH);
                    double columnWidth = parameterColumnWidth.AsDouble();

                    #endregion

                    #region Типы армирования


                    //Тип арматуры, который будет использоваться ВСЕГДА, потому что он есть во всех условиях армирования
                    RebarBarType firstMainBarTapes = _view.GetFirstMainBarTapes();
                    double diameterFirstMainRebar = firstMainBarTapes.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
                    #endregion

                    
                    List<Rebar> rebars = new List<Rebar>();
                    #region Прямые выпуски
                    if (this._activeRebarOutletTypes == "radioButMainWeldingRods")
                    {
                        /*Сначало создаю стержень в центре колонны, затем сделанны стержень придется передвигать в место его размещения
                         * сделано это для избежания ошибок в создание стержня, бывают случаи когда вроде бы стержень должен сразу создаться по месту его размещения, 
                         * но не создается по неизвестной причине, которую нужно будет выяснить*/


                        XYZ pointLower = new XYZ(elementOrigin.X, elementOrigin.Y, elementOrigin.Z + this._bottomOffsetCrossBars);
                        XYZ pointTop = new XYZ(elementOrigin.X, elementOrigin.Y, elementOrigin.Z + elementLength + _overlapThickness + _rebarOutletsLength);
                        Curve lineRebar = Line.CreateBound(pointLower, pointTop);
                        List<Curve> linesRebar = new List<Curve>() { lineRebar };

                        // Нижний Левый угол
                        Rebar rebarLowerLeft = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, firstMainBarTapes, element, linesRebar);
                        XYZ pointLowerLeft = new XYZ(
                            -0.5 * columnWidth + rebarCorver + 0.5 * diameterFirstMainRebar,
                            -0.5 * columnHeight + rebarCorver + 0.5 * diameterFirstMainRebar,
                            0);
                        ElementTransformUtils.MoveElement(doc, rebarLowerLeft.Id, pointLowerLeft);
                        rebars.Add(rebarLowerLeft);

                        // Нижний Правый угол
                        Rebar rebarLowerRight = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, firstMainBarTapes, element, linesRebar);
                        XYZ pointLowerRight = new XYZ(
                             0.5 * columnWidth - rebarCorver - 0.5 * diameterFirstMainRebar,
                             -0.5 * columnHeight + rebarCorver + 0.5 * diameterFirstMainRebar,
                             0);
                        ElementTransformUtils.MoveElement(doc, rebarLowerRight.Id, pointLowerRight);
                        rebars.Add(rebarLowerRight);

                        // Верхний Левый угол
                        Rebar rebarTopLeft = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, firstMainBarTapes, element, linesRebar);
                        XYZ pointTopLeft = new XYZ(
                            -0.5 * columnWidth + rebarCorver + 0.5 * diameterFirstMainRebar,
                            0.5 * columnHeight - rebarCorver - 0.5 * diameterFirstMainRebar,
                            0);
                        ElementTransformUtils.MoveElement(doc, rebarTopLeft.Id, pointTopLeft);
                        rebars.Add(rebarTopLeft);

                        // Верхний Правый угол
                        Rebar rebarTopRight = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, firstMainBarTapes, element, linesRebar);
                        XYZ pointTopRight = new XYZ(
                           0.5 * columnWidth - rebarCorver - 0.5 * diameterFirstMainRebar,
                           0.5 * columnHeight - rebarCorver - 0.5 * diameterFirstMainRebar,
                           0);
                        ElementTransformUtils.MoveElement(doc, rebarTopRight.Id, pointTopRight);
                        rebars.Add(rebarTopRight);


                        if (this._activeReinforcementType == "radioButRebarType2" ||
                            this._activeReinforcementType == "radioButRebarType3" ||
                            this._activeReinforcementType == "radioButRebarType5")
                        {
                            RebarBarType secondMainBarTapes = _view.GetSecondMainBarTapes();
                            double diameterSecondMainRebar = secondMainBarTapes.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();

                            // Нижний Центральный стержень
                            Rebar rebarLowerСenter = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, secondMainBarTapes, element, linesRebar);
                            XYZ pointLowerСenter = new XYZ(0, -0.5 * columnHeight + rebarCorver + 0.5 * diameterSecondMainRebar, 0);
                            ElementTransformUtils.MoveElement(doc, rebarLowerСenter.Id, pointLowerСenter);
                            rebars.Add(rebarLowerСenter);

                            // Верхний Центральный стержень
                            Rebar rebarTopСenter = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, secondMainBarTapes, element, linesRebar);
                            XYZ pointTopСenter = new XYZ(0, +0.5 * columnHeight - rebarCorver - 0.5 * diameterSecondMainRebar, 0);
                            ElementTransformUtils.MoveElement(doc, rebarTopСenter.Id, pointTopСenter);
                            rebars.Add(rebarTopСenter);

                            // Левый Центральный стержень
                            Rebar rebarLeftСenter = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, secondMainBarTapes, element, linesRebar);
                            XYZ pointLeftСenter = new XYZ(-0.5 * columnWidth + rebarCorver + 0.5 * diameterSecondMainRebar, 0, 0);
                            ElementTransformUtils.MoveElement(doc, rebarLeftСenter.Id, pointLeftСenter);
                            rebars.Add(rebarLeftСenter);

                            // Правый Центральный стержень
                            Rebar rebarRightСenter = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, secondMainBarTapes, element, linesRebar);
                            XYZ pointRightСenter = new XYZ(0.5 * columnWidth - rebarCorver - 0.5 * diameterSecondMainRebar, 0, 0);
                            ElementTransformUtils.MoveElement(doc, rebarRightСenter.Id, pointRightСenter);
                            rebars.Add(rebarRightСenter);
                        }
                        if (this._activeReinforcementType == "radioButRebarType4" ||
                            this._activeReinforcementType == "radioButRebarType5" ||
                            this._activeReinforcementType == "radioButRebarType6")
                        {
                            RebarBarType secondMainBarTapes = _view.GetSecondMainBarTapes();
                            double diameterSecondMaiRebar = secondMainBarTapes.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();

                            // Отступы стержней
                            double offsetSecondСenterLeftRebar_1 = _view.GetValueOffsetSecondСenterLeftRebar_1() / 304.8;
                            double offsetSecondСenterRightRebar_1 = _view.GetValueOffsetSecondСenterRightRebar_1() / 304.8;
                            double offsetSecondСenterTopRebar_1 = _view.GetValueOffsetSecondСenterTopRebar_1() / 304.8;
                            double offsetSecondСenterLowerRebar_1 = _view.GetValueOffsetSecondСenterLowerRebar_1() / 304.8;


                            // Нижний Центрально-Левый стержень 1
                            Rebar rebarLowerСenterLeft_1 = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, secondMainBarTapes, element, linesRebar);
                            XYZ pointLowerСenterLeft_1 = new XYZ(-offsetSecondСenterLeftRebar_1,
                                -0.5 * columnHeight + rebarCorver + 0.5 * diameterSecondMaiRebar, 0);
                            ElementTransformUtils.MoveElement(doc, rebarLowerСenterLeft_1.Id, pointLowerСenterLeft_1);
                            rebars.Add(rebarLowerСenterLeft_1);

                            // Нижний Центрально-Правый стержень 1
                            Rebar rebarLowerСenterRight_1 = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, secondMainBarTapes, element, linesRebar);
                            XYZ pointLowerСenterRight_1 = new XYZ(offsetSecondСenterRightRebar_1,
                                -0.5 * columnHeight + rebarCorver + 0.5 * diameterSecondMaiRebar, 0);
                            ElementTransformUtils.MoveElement(doc, rebarLowerСenterRight_1.Id, pointLowerСenterRight_1);
                            rebars.Add(rebarLowerСenterRight_1);

                            // Верхний Центрально-Левый стержень 1
                            Rebar rebarTopСenterLeft_1 = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, secondMainBarTapes, element, linesRebar);
                            XYZ pointTopСenterLeft_1 = new XYZ(-offsetSecondСenterLeftRebar_1,
                                0.5 * columnHeight - rebarCorver - 0.5 * diameterSecondMaiRebar, 0);
                            ElementTransformUtils.MoveElement(doc, rebarTopСenterLeft_1.Id, pointTopСenterLeft_1);
                            rebars.Add(rebarTopСenterLeft_1);

                            // Верхний Центрально-Правый стержень 1
                            Rebar rebarTopСenterRight_1 = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, secondMainBarTapes, element, linesRebar);
                            XYZ pointTopСenterRight_1 = new XYZ(offsetSecondСenterRightRebar_1,
                                0.5 * columnHeight - rebarCorver - 0.5 * diameterSecondMaiRebar, 0);
                            ElementTransformUtils.MoveElement(doc, rebarTopСenterRight_1.Id, pointTopСenterRight_1);
                            rebars.Add(rebarTopСenterRight_1);

                            // Левый Центрально-Верхний стержень 1
                            Rebar rebarLeftСenterTop_1 = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, secondMainBarTapes, element, linesRebar);
                            XYZ pointLeftСenterTop_1 = new XYZ(-0.5 * columnWidth + rebarCorver + 0.5 * diameterSecondMaiRebar,
                                offsetSecondСenterTopRebar_1, 0);
                            ElementTransformUtils.MoveElement(doc, rebarLeftСenterTop_1.Id, pointLeftСenterTop_1);
                            rebars.Add(rebarLeftСenterTop_1);

                            // Левый Центрально-Нижний стержень 1
                            Rebar rebarLeftСenterLower_1 = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, secondMainBarTapes, element, linesRebar);
                            XYZ pointLeftСenterLower_1 = new XYZ(-0.5 * columnWidth + rebarCorver + 0.5 * diameterSecondMaiRebar,
                                -offsetSecondСenterLowerRebar_1, 0);
                            ElementTransformUtils.MoveElement(doc, rebarLeftСenterLower_1.Id, pointLeftСenterLower_1);
                            rebars.Add(rebarLeftСenterLower_1);

                            // Правый Центрально-Верхний стержень 1
                            Rebar rebarRightСenterTop_1 = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, secondMainBarTapes, element, linesRebar);
                            XYZ pointRightСenterTop_1 = new XYZ(0.5 * columnWidth - rebarCorver - 0.5 * diameterSecondMaiRebar,
                                offsetSecondСenterTopRebar_1, 0);
                            ElementTransformUtils.MoveElement(doc, rebarRightСenterTop_1.Id, pointRightСenterTop_1);
                            rebars.Add(rebarRightСenterTop_1);


                            // Правый Центрально-Нижний стержень 1
                            Rebar rebarRightСenterLower_1 = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, secondMainBarTapes, element, linesRebar);
                            XYZ pointRightСenterLower_1 = new XYZ(0.5 * columnWidth - rebarCorver - 0.5 * diameterSecondMaiRebar,
                                -offsetSecondСenterLowerRebar_1, 0);
                            ElementTransformUtils.MoveElement(doc, rebarRightСenterLower_1.Id, pointRightСenterLower_1);
                            rebars.Add(rebarRightСenterLower_1);
                            if (this._activeReinforcementType == "radioButRebarType6")
                            {
                                double offsetSecondСenterLeftRebar_2 = _view.GetValueOffsetSecondСenterLeftRebar_2() / 304.8;
                                double offsetSecondСenterRightRebar_2 = _view.GetValueOffsetSecondСenterRightRebar_2() / 304.8;
                                double offsetSecondСenterTopRebar_2 = _view.GetValueOffsetSecondСenterTopRebar_2() / 304.8;
                                double offsetSecondСenterLowerRebar_2 = _view.GetValueOffsetSecondСenterLowerRebar_2() / 304.8;

                                // Нижний Центрально-Левый стержень 2
                                Rebar rebarLowerСenterLeft_2 = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, secondMainBarTapes, element, linesRebar);
                                XYZ pointLowerСenterLeft_2 = new XYZ(-offsetSecondСenterLeftRebar_1 - offsetSecondСenterLeftRebar_2,
                                    -0.5 * columnHeight + rebarCorver + 0.5 * diameterSecondMaiRebar, 0);
                                ElementTransformUtils.MoveElement(doc, rebarLowerСenterLeft_2.Id, pointLowerСenterLeft_2);
                                rebars.Add(rebarLowerСenterLeft_2);

                                // Нижний Центрально-Правый стержень 2
                                Rebar rebarLowerСenterRight_2 = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, secondMainBarTapes, element, linesRebar);
                                XYZ pointLowerСenterRight_2 = new XYZ(offsetSecondСenterRightRebar_1 + offsetSecondСenterRightRebar_2,
                                    -0.5 * columnHeight + rebarCorver + 0.5 * diameterSecondMaiRebar, 0);
                                ElementTransformUtils.MoveElement(doc, rebarLowerСenterRight_2.Id, pointLowerСenterRight_2);
                                rebars.Add(rebarLowerСenterRight_2);

                                // Верхний Центрально-Левый стержень 2
                                Rebar rebarTopСenterLeft_2 = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, secondMainBarTapes, element, linesRebar);
                                XYZ pointTopСenterLeft_2 = new XYZ(-offsetSecondСenterLeftRebar_1 - offsetSecondСenterLeftRebar_2,
                                    0.5 * columnHeight - rebarCorver - 0.5 * diameterSecondMaiRebar, 0);
                                ElementTransformUtils.MoveElement(doc, rebarTopСenterLeft_2.Id, pointTopСenterLeft_2);
                                rebars.Add(rebarTopСenterLeft_2);

                                // Верхний Центрально-Правый стержень 2
                                Rebar rebarTopСenterRight_2 = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, secondMainBarTapes, element, linesRebar);
                                XYZ pointTopСenterRight_2 = new XYZ(offsetSecondСenterRightRebar_1 + offsetSecondСenterRightRebar_2,
                                    0.5 * columnHeight - rebarCorver - 0.5 * diameterSecondMaiRebar, 0);
                                ElementTransformUtils.MoveElement(doc, rebarTopСenterRight_2.Id, pointTopСenterRight_2);
                                rebars.Add(rebarTopСenterRight_2);

                                // Левый Центрально-Верхний стержень 2
                                Rebar rebarLeftСenterTop_2 = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, secondMainBarTapes, element, linesRebar);
                                XYZ pointLeftСenterTop_2 = new XYZ(-0.5 * columnWidth + rebarCorver + 0.5 * diameterSecondMaiRebar,
                                    offsetSecondСenterTopRebar_1 + offsetSecondСenterTopRebar_2, 0);
                                ElementTransformUtils.MoveElement(doc, rebarLeftСenterTop_2.Id, pointLeftСenterTop_2);
                                rebars.Add(rebarLeftСenterTop_2);

                                // Левый Центрально-Нижний стержень 2
                                Rebar rebarLeftСenterLower_2 = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, secondMainBarTapes, element, linesRebar);
                                XYZ pointLeftСenterLower_2 = new XYZ(-0.5 * columnWidth + rebarCorver + 0.5 * diameterSecondMaiRebar,
                                    -offsetSecondСenterLowerRebar_1 - offsetSecondСenterLowerRebar_2, 0);
                                ElementTransformUtils.MoveElement(doc, rebarLeftСenterLower_2.Id, pointLeftСenterLower_2);
                                rebars.Add(rebarLeftСenterLower_2);

                                // Правый Центрально-Верхний стержень 2
                                Rebar rebarRightСenterTop_2 = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, secondMainBarTapes, element, linesRebar);
                                XYZ pointRightСenterTop_2 = new XYZ(0.5 * columnWidth - rebarCorver - 0.5 * diameterSecondMaiRebar,
                                    offsetSecondСenterTopRebar_1 + offsetSecondСenterTopRebar_2, 0);
                                ElementTransformUtils.MoveElement(doc, rebarRightСenterTop_2.Id, pointRightСenterTop_2);
                                rebars.Add(rebarRightСenterTop_2);

                                // Правый Центрально-Нижний стержень 1
                                Rebar rebarRightСenterLower_2 = _creatureRebarFromCurvesAndShape(doc, rebarSmoothShape, secondMainBarTapes, element, linesRebar);
                                XYZ pointRightСenterLower_2 = new XYZ(0.5 * columnWidth - rebarCorver - 0.5 * diameterSecondMaiRebar,
                                    -offsetSecondСenterLowerRebar_1 - offsetSecondСenterLowerRebar_2, 0);
                                ElementTransformUtils.MoveElement(doc, rebarRightСenterLower_2.Id, pointRightСenterLower_2);
                                rebars.Add(rebarRightСenterLower_2);
                            }
                        }
                    }
                    #endregion

                    #region Изогнутые выпуски

                    else if (this._activeRebarOutletTypes == "radioButMainOverlappingRods")
                    {
                        /* Защита от дурака! Если выбраное дополнительное смещение загиба и диаметр стержня в сумме будут равны нулю и этим
                         * отменять загиб, то мы покажем пользователю в чем ошибка и значение доп. смещения назначем ноль !*/
                        if (diameterFirstMainRebar + _additionalOffsetBendBar == 0)
                        {
                            _view.SetColorTextBoxAdditionalOffsetBendBar("Red");
                            MessageBox.Show("Веденно некоректное значение изгиба стрежня: диаметр стержня и значения изгиба не должны быть равны нулю," +
                                " при создание стержней значение параметра дополнительного смещения учитываться не будет !");
                            _additionalOffsetBendBar = 0;
                            _view.SetColorTextBoxAdditionalOffsetBendBar("White");
                        }

                        // Точки для построении линий формы основной арматуры, будут использоваться всегда, в независимости от типа армирования
                        XYZ pointFirst_1 = new XYZ(elementOrigin.X, elementOrigin.Y, elementOrigin.Z + _bottomOffsetCrossBars);
                        XYZ pointFirst_2 = new XYZ(elementOrigin.X, elementOrigin.Y, elementOrigin.Z + elementLength);
                        XYZ pointFirst_3 = new XYZ(
                            elementOrigin.X + diameterFirstMainRebar + _additionalOffsetBendBar,
                            elementOrigin.Y,
                            elementOrigin.Z + elementLength + _overlapThickness);
                        XYZ pointFirst_4 = new XYZ(
                                 elementOrigin.X + diameterFirstMainRebar + _additionalOffsetBendBar,
                                 elementOrigin.Y,
                                 elementOrigin.Z + elementLength + _overlapThickness + _rebarOutletsLength);

                        Curve lineFirst_1 = Line.CreateBound(pointFirst_1, pointFirst_2);
                        Curve lineFirst_2 = Line.CreateBound(pointFirst_2, pointFirst_3);
                        Curve lineFirst_3 = Line.CreateBound(pointFirst_3, pointFirst_4);
                        List<Curve> linesFirstRebar = new List<Curve>() { lineFirst_1, lineFirst_2, lineFirst_3 };

                        Line rotateLine = Line.CreateBound(pointFirst_1, pointFirst_2);
                        bool isOutletsInside = _view.GetOutletsInside();


                        // Нижний левый угол
                        Rebar rebarLowerLeft = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, firstMainBarTapes, element, linesFirstRebar);
                        XYZ pointLowerLeft = new XYZ(-0.5 * columnWidth + rebarCorver + 0.5 * diameterFirstMainRebar,
                            -0.5 * columnHeight + rebarCorver + 0.5 * diameterFirstMainRebar,
                            0);
                        if (isOutletsInside)
                        {
                            ElementTransformUtils.RotateElement(doc, rebarLowerLeft.Id, rotateLine, 45 * (Math.PI / 180));
                        }
                        ElementTransformUtils.MoveElement(doc, rebarLowerLeft.Id, pointLowerLeft);
                        rebars.Add(rebarLowerLeft);


                        // Верхний левый угол
                        Rebar rebarTopLeft = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, firstMainBarTapes, element, linesFirstRebar);
                        XYZ pointTopLeft = new XYZ(-0.5 * columnWidth + rebarCorver + 0.5 * diameterFirstMainRebar,
                            0.5 * columnHeight - rebarCorver - 0.5 * diameterFirstMainRebar,
                            0);
                        if (isOutletsInside)
                        {
                            ElementTransformUtils.RotateElement(doc, rebarTopLeft.Id, rotateLine, -45 * (Math.PI / 180));
                        }
                        ElementTransformUtils.MoveElement(doc, rebarTopLeft.Id, pointTopLeft);
                        rebars.Add(rebarTopLeft);

                        // Нижний правый угол
                        Rebar rebarLowerRight = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, firstMainBarTapes, element, linesFirstRebar);
                        XYZ pointLowerRight = new XYZ(0.5 * columnWidth - rebarCorver - 0.5 * diameterFirstMainRebar,
                            -0.5 * columnHeight + rebarCorver + 0.5 * diameterFirstMainRebar,
                            0);
                        if (isOutletsInside)
                        {
                            ElementTransformUtils.RotateElement(doc, rebarLowerRight.Id, rotateLine, 135 * (Math.PI / 180));
                        }
                        else
                        {
                            ElementTransformUtils.RotateElement(doc, rebarLowerRight.Id, rotateLine, 180 * (Math.PI / 180));
                        }
                        ElementTransformUtils.MoveElement(doc, rebarLowerRight.Id, pointLowerRight);
                        rebars.Add(rebarLowerRight);

                        // Верхний правый угол
                        Rebar rebarTopRight = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, firstMainBarTapes, element, linesFirstRebar);
                        XYZ pointTopRight = new XYZ(0.5 * columnWidth - rebarCorver - 0.5 * diameterFirstMainRebar,
                            0.5 * columnHeight - rebarCorver - 0.5 * diameterFirstMainRebar,
                            0);
                        if (isOutletsInside)
                        {
                            ElementTransformUtils.RotateElement(doc, rebarTopRight.Id, rotateLine, -135 * (Math.PI / 180));
                        }
                        else
                        {
                            ElementTransformUtils.RotateElement(doc, rebarTopRight.Id, rotateLine, 180 * (Math.PI / 180));
                        }
                        ElementTransformUtils.MoveElement(doc, rebarTopRight.Id, pointTopRight);
                        rebars.Add(rebarTopRight);

                        if (this._activeReinforcementType != "radioButRebarType1")
                        {
                            RebarBarType secondMainBarTapes = _view.GetSecondMainBarTapes();
                            double diameterSecondMainRebar = secondMainBarTapes.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();

                            XYZ pointSecond_1 = new XYZ(elementOrigin.X, elementOrigin.Y, elementOrigin.Z + _bottomOffsetCrossBars);
                            XYZ pointSecond_2 = new XYZ(elementOrigin.X, elementOrigin.Y, elementOrigin.Z + elementLength);
                            XYZ pointSecond_3 = new XYZ(
                                elementOrigin.X + diameterFirstMainRebar + _additionalOffsetBendBar,
                                elementOrigin.Y,
                                elementOrigin.Z + elementLength + _overlapThickness);
                            XYZ pointSecond_4 = new XYZ(
                                elementOrigin.X + diameterSecondMainRebar + _additionalOffsetBendBar,
                                elementOrigin.Y,
                                elementOrigin.Z + elementLength + _overlapThickness + _rebarOutletsLength);

                            Curve lineSecond_1 = Line.CreateBound(pointSecond_1, pointSecond_2);
                            Curve lineSecond_2 = Line.CreateBound(pointSecond_2, pointSecond_3);
                            Curve lineSecond_3 = Line.CreateBound(pointSecond_3, pointSecond_4);
                            List<Curve> linesSecondRebar = new List<Curve>() { lineSecond_1, lineSecond_2, lineSecond_3 };

                            if (this._activeReinforcementType == "radioButRebarType2" ||
                                this._activeReinforcementType == "radioButRebarType3" ||
                                this._activeReinforcementType == "radioButRebarType5")
                            {
                                // Нижний Центральный стержень
                                Rebar rebarLowerСenter = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, secondMainBarTapes, element, linesSecondRebar);
                                XYZ pointLowerСenter = new XYZ(0, -0.5 * columnHeight + rebarCorver + 0.5 * diameterSecondMainRebar, 0);
                                if (isOutletsInside)
                                {
                                    ElementTransformUtils.RotateElement(doc, rebarLowerСenter.Id, rotateLine, 90 * (Math.PI / 180));
                                }
                                ElementTransformUtils.MoveElement(doc, rebarLowerСenter.Id, pointLowerСenter);
                                rebars.Add(rebarLowerСenter);

                                // Верхний Центральный стержень
                                Rebar rebarTopСenter = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, secondMainBarTapes, element, linesSecondRebar);
                                XYZ pointTopСenter = new XYZ(0, +0.5 * columnHeight - rebarCorver - 0.5 * diameterSecondMainRebar, 0);
                                if (isOutletsInside)
                                {
                                    ElementTransformUtils.RotateElement(doc, rebarTopСenter.Id, rotateLine, -90 * (Math.PI / 180));
                                }
                                ElementTransformUtils.MoveElement(doc, rebarTopСenter.Id, pointTopСenter);
                                rebars.Add(rebarTopСenter);

                                // Левый Центральный стержень
                                Rebar rebarLeftСenter = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, secondMainBarTapes, element, linesSecondRebar);
                                XYZ pointLeftСenter = new XYZ(-0.5 * columnWidth + rebarCorver + 0.5 * diameterSecondMainRebar, 0, 0);
                                if (!isOutletsInside)
                                {
                                    ElementTransformUtils.RotateElement(doc, rebarLeftСenter.Id, rotateLine, 90 * (Math.PI / 180));
                                }
                                ElementTransformUtils.MoveElement(doc, rebarLeftСenter.Id, pointLeftСenter);
                                rebars.Add(rebarLeftСenter);

                                // Правый Центральный стержень
                                Rebar rebarRightСenter = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, secondMainBarTapes, element, linesSecondRebar);
                                XYZ pointRightСenter = new XYZ(0.5 * columnWidth - rebarCorver - 0.5 * diameterSecondMainRebar, 0, 0);
                                if (!isOutletsInside)
                                {
                                    ElementTransformUtils.RotateElement(doc, rebarRightСenter.Id, rotateLine, 90 * (Math.PI / 180));
                                }
                                else
                                {
                                    ElementTransformUtils.RotateElement(doc, rebarRightСenter.Id, rotateLine, 180 * (Math.PI / 180));

                                }
                                ElementTransformUtils.MoveElement(doc, rebarRightСenter.Id, pointRightСenter);
                                rebars.Add(rebarRightСenter);
                            }
                            if (this._activeReinforcementType == "radioButRebarType4" ||
                                this._activeReinforcementType == "radioButRebarType5" ||
                                this._activeReinforcementType == "radioButRebarType6")
                            {
                                double offsetSecondСenterLeftRebar_1 = _view.GetValueOffsetSecondСenterLeftRebar_1() / 304.8;
                                double offsetSecondСenterRightRebar_1 = _view.GetValueOffsetSecondСenterRightRebar_1() / 304.8;
                                double offsetSecondСenterTopRebar_1 = _view.GetValueOffsetSecondСenterTopRebar_1() / 304.8;
                                double offsetSecondСenterLowerRebar_1 = _view.GetValueOffsetSecondСenterLowerRebar_1() / 304.8;



                                // Нижний Центрально-Левый стержень 1
                                Rebar rebarLowerСenterLeft_1 = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, secondMainBarTapes, element, linesSecondRebar);
                                XYZ pointLowerСenterLeft_1 = new XYZ(
                                    -offsetSecondСenterLeftRebar_1,
                                    -0.5 * columnHeight + rebarCorver + 0.5 * diameterSecondMainRebar,
                                    0);
                                if (isOutletsInside)
                                {
                                    ElementTransformUtils.RotateElement(doc, rebarLowerСenterLeft_1.Id, rotateLine, 90 * (Math.PI / 180));
                                }
                                ElementTransformUtils.MoveElement(doc, rebarLowerСenterLeft_1.Id, pointLowerСenterLeft_1);
                                rebars.Add(rebarLowerСenterLeft_1);

                                // Нижний Центрально-Правый стержень 1
                                Rebar rebarLowerСenterRight_1 = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, secondMainBarTapes, element, linesSecondRebar);
                                XYZ pointLowerСenterRight_1 = new XYZ(
                                    offsetSecondСenterRightRebar_1,
                                    -0.5 * columnHeight + rebarCorver + 0.5 * diameterSecondMainRebar,
                                    0);
                                if (isOutletsInside)
                                {
                                    ElementTransformUtils.RotateElement(doc, rebarLowerСenterRight_1.Id, rotateLine, 90 * (Math.PI / 180));
                                }
                                else
                                {
                                    ElementTransformUtils.RotateElement(doc, rebarLowerСenterRight_1.Id, rotateLine, 180 * (Math.PI / 180));
                                }
                                ElementTransformUtils.MoveElement(doc, rebarLowerСenterRight_1.Id, pointLowerСenterRight_1);
                                rebars.Add(rebarLowerСenterRight_1);

                                // Верхний Центрально-Левый стержень 1
                                Rebar rebarTopСenterLeft_1 = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, secondMainBarTapes, element, linesSecondRebar);
                                XYZ pointTopСenterLeft_1 = new XYZ(-offsetSecondСenterLeftRebar_1,
                                        0.5 * columnHeight - rebarCorver - 0.5 * diameterSecondMainRebar, 0);
                                if (isOutletsInside)
                                {
                                    ElementTransformUtils.RotateElement(doc, rebarTopСenterLeft_1.Id, rotateLine, -90 * (Math.PI / 180));
                                }
                                ElementTransformUtils.MoveElement(doc, rebarTopСenterLeft_1.Id, pointTopСenterLeft_1);
                                rebars.Add(rebarTopСenterLeft_1);

                                // Верхний Центрально-Правый стержень 1
                                Rebar rebarTopСenterRight_1 = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, secondMainBarTapes, element, linesSecondRebar);
                                XYZ pointTopСenterRight_1 = new XYZ(offsetSecondСenterRightRebar_1,
                                       0.5 * columnHeight - rebarCorver - 0.5 * diameterSecondMainRebar, 0);
                                if (isOutletsInside)
                                {
                                    ElementTransformUtils.RotateElement(doc, rebarTopСenterRight_1.Id, rotateLine, -90 * (Math.PI / 180));
                                }
                                else
                                {
                                    ElementTransformUtils.RotateElement(doc, rebarTopСenterRight_1.Id, rotateLine, -180 * (Math.PI / 180));

                                }
                                ElementTransformUtils.MoveElement(doc, rebarTopСenterRight_1.Id, pointTopСenterRight_1);
                                rebars.Add(rebarTopСenterRight_1);

                                // Левый Центрально-Верхний стержень 1
                                Rebar rebarLeftСenterTop_1 = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, secondMainBarTapes, element, linesSecondRebar);
                                XYZ pointLeftСenterTop_1 = new XYZ(-0.5 * columnWidth + rebarCorver + 0.5 * diameterSecondMainRebar,
                                        offsetSecondСenterTopRebar_1, 0);
                                if (!isOutletsInside)
                                {
                                    ElementTransformUtils.RotateElement(doc, rebarLeftСenterTop_1.Id, rotateLine, -90 * (Math.PI / 180));
                                }
                                ElementTransformUtils.MoveElement(doc, rebarLeftСenterTop_1.Id, pointLeftСenterTop_1);
                                rebars.Add(rebarLeftСenterTop_1);

                                // Левый Центрально-Нижний стержень 1
                                Rebar rebarLeftСenterLower_1 = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, secondMainBarTapes, element, linesSecondRebar);
                                XYZ pointLeftСenterLower_1 = new XYZ(-0.5 * columnWidth + rebarCorver + 0.5 * diameterSecondMainRebar,
                                        -offsetSecondСenterLowerRebar_1, 0);
                                if (!isOutletsInside)
                                {
                                    ElementTransformUtils.RotateElement(doc, rebarLeftСenterLower_1.Id, rotateLine, 90 * (Math.PI / 180));
                                }
                                ElementTransformUtils.MoveElement(doc, rebarLeftСenterLower_1.Id, pointLeftСenterLower_1);
                                rebars.Add(rebarLeftСenterLower_1);

                                // Правый Центрально-Верхний стержень 1
                                Rebar rebarRightСenterTop_1 = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, secondMainBarTapes, element, linesSecondRebar);
                                XYZ pointRightСenterTop_1 = new XYZ(0.5 * columnWidth - rebarCorver - 0.5 * diameterSecondMainRebar,
                                       offsetSecondСenterTopRebar_1, 0);
                                if (isOutletsInside)
                                {
                                    ElementTransformUtils.RotateElement(doc, rebarRightСenterTop_1.Id, rotateLine, -180 * (Math.PI / 180));
                                }
                                else
                                {
                                    ElementTransformUtils.RotateElement(doc, rebarRightСenterTop_1.Id, rotateLine, -90 * (Math.PI / 180));
                                }
                                ElementTransformUtils.MoveElement(doc, rebarRightСenterTop_1.Id, pointRightСenterTop_1);
                                rebars.Add(rebarRightСenterTop_1);

                                // Правый Центрально-Нижний стержень 1
                                Rebar rebarRightСenterLower_1 = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, secondMainBarTapes, element, linesSecondRebar);
                                XYZ pointRightСenterLower_1 = new XYZ(0.5 * columnWidth - rebarCorver - 0.5 * diameterSecondMainRebar,
                                       -offsetSecondСenterLowerRebar_1, 0);
                                if (isOutletsInside)
                                {
                                    ElementTransformUtils.RotateElement(doc, rebarRightСenterLower_1.Id, rotateLine, 180 * (Math.PI / 180));
                                }
                                else
                                {
                                    ElementTransformUtils.RotateElement(doc, rebarRightСenterLower_1.Id, rotateLine, 90 * (Math.PI / 180));
                                }
                                ElementTransformUtils.MoveElement(doc, rebarRightСenterLower_1.Id, pointRightСenterLower_1);
                                rebars.Add(rebarRightСenterLower_1);

                                if (this._activeReinforcementType == "radioButRebarType6")
                                {
                                    double offsetSecondСenterLeftRebar_2 = _view.GetValueOffsetSecondСenterLeftRebar_2() / 304.8;
                                    double offsetSecondСenterRightRebar_2 = _view.GetValueOffsetSecondСenterRightRebar_2() / 304.8;
                                    double offsetSecondСenterTopRebar_2 = _view.GetValueOffsetSecondСenterTopRebar_2() / 304.8;
                                    double offsetSecondСenterLowerRebar_2 = _view.GetValueOffsetSecondСenterLowerRebar_2() / 304.8;

                                    // Нижний Центрально-Левый стержень 2
                                    Rebar rebarLowerСenterLeft_2 = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, secondMainBarTapes, element, linesSecondRebar);
                                    XYZ pointLowerСenterLeft_2 = new XYZ(-offsetSecondСenterLeftRebar_1 - offsetSecondСenterLeftRebar_2,
                                            -0.5 * columnHeight + rebarCorver + 0.5 * diameterSecondMainRebar, 0);
                                    if (isOutletsInside)
                                    {
                                        ElementTransformUtils.RotateElement(doc, rebarLowerСenterLeft_2.Id, rotateLine, 90 * (Math.PI / 180));
                                    }
                                    ElementTransformUtils.MoveElement(doc, rebarLowerСenterLeft_2.Id, pointLowerСenterLeft_2);
                                    rebars.Add(rebarLowerСenterLeft_2);

                                    // Нижний Центрально-Правый стержень 2
                                    Rebar rebarLowerСenterRight_2 = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, secondMainBarTapes, element, linesSecondRebar);
                                    XYZ pointLowerСenterRight_2 = new XYZ(
                                        offsetSecondСenterRightRebar_1 + offsetSecondСenterRightRebar_2,
                                        -0.5 * columnHeight + rebarCorver + 0.5 * diameterSecondMainRebar, 0);
                                    if (isOutletsInside)
                                    {
                                        ElementTransformUtils.RotateElement(doc, rebarLowerСenterRight_2.Id, rotateLine, 90 * (Math.PI / 180));
                                    }
                                    else
                                    {
                                        ElementTransformUtils.RotateElement(doc, rebarLowerСenterRight_2.Id, rotateLine, 180 * (Math.PI / 180));
                                    }
                                    ElementTransformUtils.MoveElement(doc, rebarLowerСenterRight_2.Id, pointLowerСenterRight_2);
                                    rebars.Add(rebarLowerСenterRight_2);

                                    // Верхний Центрально-Левый стержень 2
                                    Rebar rebarTopСenterLeft_2 = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, secondMainBarTapes, element, linesSecondRebar);
                                    XYZ pointTopСenterLeft_2 = new XYZ(
                                        -offsetSecondСenterLeftRebar_1 - offsetSecondСenterLeftRebar_2,
                                        0.5 * columnHeight - rebarCorver - 0.5 * diameterSecondMainRebar, 0);
                                    if (isOutletsInside)
                                    {
                                        ElementTransformUtils.RotateElement(doc, rebarTopСenterLeft_2.Id, rotateLine, -90 * (Math.PI / 180));
                                    }
                                    ElementTransformUtils.MoveElement(doc, rebarTopСenterLeft_2.Id, pointTopСenterLeft_2);
                                    rebars.Add(rebarTopСenterLeft_2);

                                    // Верхний Центрально-Правый стержень 2
                                    Rebar rebarTopСenterRight_2 = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, secondMainBarTapes, element, linesSecondRebar);
                                    XYZ pointTopСenterRight_2 = new XYZ(
                                        offsetSecondСenterRightRebar_1 + offsetSecondСenterRightRebar_2,
                                            0.5 * columnHeight - rebarCorver - 0.5 * diameterSecondMainRebar, 0);
                                    if (isOutletsInside)
                                    {
                                        ElementTransformUtils.RotateElement(doc, rebarTopСenterRight_2.Id, rotateLine, -90 * (Math.PI / 180));
                                    }
                                    else
                                    {
                                        ElementTransformUtils.RotateElement(doc, rebarTopСenterRight_2.Id, rotateLine, -180 * (Math.PI / 180));

                                    }
                                    ElementTransformUtils.MoveElement(doc, rebarTopСenterRight_2.Id, pointTopСenterRight_2);
                                    rebars.Add(rebarTopСenterRight_2);

                                    // Левый Центрально-Верхний стержень 2
                                    Rebar rebarLeftСenterTop_2 = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, secondMainBarTapes, element, linesSecondRebar);
                                    XYZ pointLeftСenterTop_2 = new XYZ(
                                        -0.5 * columnWidth + rebarCorver + 0.5 * diameterSecondMainRebar,
                                            offsetSecondСenterTopRebar_1 + offsetSecondСenterTopRebar_2, 0);
                                    if (!isOutletsInside)
                                    {
                                        ElementTransformUtils.RotateElement(doc, rebarLeftСenterTop_2.Id, rotateLine, -90 * (Math.PI / 180));
                                    }
                                    ElementTransformUtils.MoveElement(doc, rebarLeftСenterTop_2.Id, pointLeftСenterTop_2);
                                    rebars.Add(rebarLeftСenterTop_2);

                                    // Левый Центрально-Нижний стержень 2
                                    Rebar rebarLeftСenterLower_2 = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, secondMainBarTapes, element, linesSecondRebar);
                                    XYZ pointLeftСenterLower_2 = new XYZ(
                                        -0.5 * columnWidth + rebarCorver + 0.5 * diameterSecondMainRebar,
                                            -offsetSecondСenterLowerRebar_1 - offsetSecondСenterLowerRebar_2, 0);
                                    if (!isOutletsInside)
                                    {
                                        ElementTransformUtils.RotateElement(doc, rebarLeftСenterLower_2.Id, rotateLine, 90 * (Math.PI / 180));
                                    }
                                    ElementTransformUtils.MoveElement(doc, rebarLeftСenterLower_2.Id, pointLeftСenterLower_2);
                                    rebars.Add(rebarLeftСenterLower_2);

                                    // Правый Центрально-Верхний стержень 2
                                    Rebar rebarRightСenterTop_2 = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, secondMainBarTapes, element, linesSecondRebar);
                                    XYZ pointRightСenterTop_2 = new XYZ(0.5 * columnWidth - rebarCorver - 0.5 * diameterSecondMainRebar,
                                           offsetSecondСenterTopRebar_1 + offsetSecondСenterTopRebar_2, 0);
                                    if (isOutletsInside)
                                    {
                                        ElementTransformUtils.RotateElement(doc, rebarRightСenterTop_2.Id, rotateLine, -180 * (Math.PI / 180));
                                    }
                                    else
                                    {
                                        ElementTransformUtils.RotateElement(doc, rebarRightСenterTop_2.Id, rotateLine, -90 * (Math.PI / 180));
                                    }
                                    ElementTransformUtils.MoveElement(doc, rebarRightСenterTop_2.Id, pointRightСenterTop_2);
                                    rebars.Add(rebarRightСenterTop_2);

                                    // Правый Центрально-Нижний стержень 2
                                    Rebar rebarRightСenterLower_2 = _creatureRebarFromCurvesAndShape(doc, rebarCurvedShape, secondMainBarTapes, element, linesSecondRebar);
                                    XYZ pointRightСenterLower_2 = new XYZ(0.5 * columnWidth - rebarCorver - 0.5 * diameterSecondMainRebar,
                                            -offsetSecondСenterLowerRebar_1 - offsetSecondСenterLowerRebar_2, 0);
                                    if (isOutletsInside)
                                    {
                                        ElementTransformUtils.RotateElement(doc, rebarRightСenterLower_2.Id, rotateLine, 180 * (Math.PI / 180));
                                    }
                                    else
                                    {
                                        ElementTransformUtils.RotateElement(doc, rebarRightСenterLower_2.Id, rotateLine, 90 * (Math.PI / 180));
                                    }
                                    ElementTransformUtils.MoveElement(doc, rebarRightСenterLower_2.Id, pointRightСenterLower_2);
                                    rebars.Add(rebarRightСenterLower_2);
                                }
                            }
                        }
                    }
                    #endregion

                    #region Хомуты

                    #region Основной хомут
                    RebarBarType basicClampBarTapes = _view.GetBasicClampBarTapes();
                    double diameterBasicClamp = basicClampBarTapes.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();

                    // Нижний Левый угол
                    XYZ basicClampPoint_1 = new XYZ(
                        elementOrigin.X - 0.5 * columnWidth + rebarCorver - 0.5 * diameterBasicClamp,
                        elementOrigin.Y - 0.5 * columnHeight + rebarCorver - 0.5 * diameterBasicClamp,
                        elementOrigin.Z + _bottomOffsetBasicClamp
                        );

                    // Верхний Левый угол
                    XYZ basicClampPoint_2 = new XYZ(
                        elementOrigin.X - 0.5 * columnWidth + rebarCorver - 0.5 * diameterBasicClamp,
                        elementOrigin.Y + 0.5 * columnHeight - rebarCorver + 0.5 * diameterBasicClamp,
                        elementOrigin.Z + _bottomOffsetBasicClamp
                        );

                    // Верхний Правый угол
                    XYZ basicClampPoint_3 = new XYZ(
                        elementOrigin.X + 0.5 * columnWidth - rebarCorver + 0.5 * diameterBasicClamp,
                        elementOrigin.Y + 0.5 * columnHeight - rebarCorver + 0.5 * diameterBasicClamp,
                        elementOrigin.Z + _bottomOffsetBasicClamp
                        );

                    // Нижний Правый угол
                    XYZ basicClampPoint_4 = new XYZ(
                        elementOrigin.X + 0.5 * columnWidth - rebarCorver + 0.5 * diameterBasicClamp,
                        elementOrigin.Y - 0.5 * columnHeight + rebarCorver - 0.5 * diameterBasicClamp,
                        elementOrigin.Z + _bottomOffsetBasicClamp
                        );

                    Curve lineBasicClamp_1 = Line.CreateBound(basicClampPoint_1, basicClampPoint_2);
                    Curve lineBasicClamp_2 = Line.CreateBound(basicClampPoint_2, basicClampPoint_3);
                    Curve lineBasicClamp_3 = Line.CreateBound(basicClampPoint_3, basicClampPoint_4);
                    Curve lineBasicClamp_4 = Line.CreateBound(basicClampPoint_4, basicClampPoint_1);
                    List<Curve> linesBasicClamp = new List<Curve>() { lineBasicClamp_1, lineBasicClamp_2, lineBasicClamp_3, lineBasicClamp_4};



                    Rebar rebarBasicLowerClamp = _creatureRebarClampsFromCurvesAndShape(doc, rebarClamp, basicClampBarTapes, element, linesBasicClamp);
                    int countClampLower = _view.GetValueCountLowerClamp();
                    double stepClampLower = _view.GetValueStepLowerClamp() / 304.8;

                    // Перевод шага стержней в мм (для случаев если параметры единицы проекта для интервалов стержней, не в мм.
                    Parameter stepLowerClamp = rebarBasicLowerClamp.get_Parameter(BuiltInParameter.REBAR_ELEM_BAR_SPACING);
                    DisplayUnitType displayUnitType = stepLowerClamp.DisplayUnitType;
                    displayUnitType = DisplayUnitType.DUT_MILLIMETERS;

                    rebarBasicLowerClamp.get_Parameter(BuiltInParameter.REBAR_ELEM_LAYOUT_RULE).Set(3);
                    rebarBasicLowerClamp.get_Parameter(BuiltInParameter.REBAR_ELEM_QUANTITY_OF_BARS).Set(countClampLower + 1);
                    rebarBasicLowerClamp.get_Parameter(BuiltInParameter.REBAR_ELEM_BAR_SPACING).Set(stepClampLower);
                    rebars.Add(rebarBasicLowerClamp);


                    Rebar rebarBasicMiddleClamp = _creatureRebarClampsFromCurvesAndShape(doc, rebarClamp, basicClampBarTapes, element, linesBasicClamp);
                    int countClampMiddle = _view.GetValueCountMiddleClamp();
                    double stepClampMiddle = _view.GetValueStepMiddleClamp() / 304.8;

                    // Перевод шага стержней в мм (для случаев если параметры единицы проекта для интервалов стержней, не в мм.
                    Parameter stepMiddleClamp = rebarBasicMiddleClamp.get_Parameter(BuiltInParameter.REBAR_ELEM_BAR_SPACING);
                    displayUnitType = stepMiddleClamp.DisplayUnitType;
                    displayUnitType = DisplayUnitType.DUT_MILLIMETERS;

                    rebarBasicMiddleClamp.get_Parameter(BuiltInParameter.REBAR_ELEM_LAYOUT_RULE).Set(3);
                    rebarBasicMiddleClamp.get_Parameter(BuiltInParameter.REBAR_ELEM_QUANTITY_OF_BARS).Set(countClampMiddle);
                    rebarBasicMiddleClamp.get_Parameter(BuiltInParameter.REBAR_ELEM_BAR_SPACING).Set(stepClampMiddle);

                    XYZ vectorMoveMiddleClamp = new XYZ(0, 0, (stepClampLower * countClampLower + stepClampMiddle));
                    ElementTransformUtils.MoveElement(doc, rebarBasicMiddleClamp.Id, vectorMoveMiddleClamp);
                    rebars.Add(rebarBasicMiddleClamp);

                    
                    Rebar rebarBasicTopClamp = _creatureRebarClampsFromCurvesAndShape(doc, rebarClamp, basicClampBarTapes, element, linesBasicClamp);
                    int countClampTop = _view.GetValueCountTopClamp();
                    double stepClampTop = _view.GetValueStepTopClamp() / 304.8;

                    // Перевод шага стержней в мм (для случаев если параметры единицы проекта для интервалов стержней, не в мм.
                    Parameter stepTopClamp = rebarBasicTopClamp.get_Parameter(BuiltInParameter.REBAR_ELEM_BAR_SPACING);
                    displayUnitType = stepTopClamp.DisplayUnitType;
                    displayUnitType = DisplayUnitType.DUT_MILLIMETERS;

                    rebarBasicTopClamp.get_Parameter(BuiltInParameter.REBAR_ELEM_LAYOUT_RULE).Set(3);
                    rebarBasicTopClamp.get_Parameter(BuiltInParameter.REBAR_ELEM_QUANTITY_OF_BARS).Set(countClampTop);
                    rebarBasicTopClamp.get_Parameter(BuiltInParameter.REBAR_ELEM_BAR_SPACING).Set(stepClampTop);

                    XYZ vectorMoveTopClamp = new XYZ(0, 0, (stepClampLower * countClampLower + stepClampMiddle * countClampMiddle + stepClampTop));
                    ElementTransformUtils.MoveElement(doc, rebarBasicTopClamp.Id, vectorMoveTopClamp);
                    rebars.Add(rebarBasicTopClamp);
                    
                    #endregion

                    #endregion
                    totalMessage++;
                    if (rebars.Count > 0)
                    {
                        if(_view.GetSolidInView() && doc.ActiveView.ViewType == ViewType.ThreeD)
                        {
                            foreach(Rebar rebar in rebars)
                            {
                                rebar.SetSolidInView((View3D)doc.ActiveView, true);
                            }
                        }
                        else
                        {
                            if (totalMessage == 0)
                            {
                                // Чтобы сообщения не повторялись по нескольку раз во время работы цикла
                                TaskDialog.Show("Предупреждение !", "Показать арматуру как тело, возможно при создании ее на 3D виде");
                            }
                        }
                        if(_view.GetShowUnoverlapped())
                        {
                            foreach (Rebar rebar in rebars)
                            {
                                rebar.SetUnobscuredInView((View3D)doc.ActiveView, true);
                            }
                        }
                        if(totalMessage == _selectedElements.Count) 
                        {
                            MessageBox.Show("Арматура успешно созданна!"); // Чтобы сообщения не повторялись по нескольку раз во время работы цикла
                        }
                    }
                }
                t.Commit();
            }
        }

        public string GetName()
        {
            return "Create rebar in the inner class";
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
            Element rebarCorverType = this.doc.GetElement(parameterRebarCorver.AsElementId());

            Parameter lenghtRebarCorver = rebarCorverType.get_Parameter(BuiltInParameter.COVER_TYPE_LENGTH);
            return lenghtRebarCorver;
        }

        /// <summary>
        /// Выбирает типы форм арматуры в проекте и возращает указаный тип арматуры по имени.
        /// </summary>
        /// <returns>
        /// Форма арматуры указанной арматуры
        /// </returns>
        /// <exception cref= "System.NullReferenceException">: Ссылка на объект не указывает на экземпляр объекта.</exception>
        private RebarShape _getRebarShape(string name)
        {
            List<RebarShape> rebarShapeList = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarShape)).Cast<RebarShape>()
                .ToList();

            foreach (RebarShape rebarShape in rebarShapeList)
            {
                if (rebarShape.Name == name)
                {
                    return rebarShape;
                }
            }
            return null;
        }

        /// <summary>
        /// Выбирает типы отгиба арматуры в проекте и возращает указаный тип отгиба арматуры по имени.
        /// </summary>
        /// <returns>
        /// Отгиб арматуры указанной арматуры
        /// </returns>
        /// <exception cref= "System.NullReferenceException">: Ссылка на объект не указывает на экземпляр объекта.</exception>
        private RebarHookType _getRebarHookType(string name)
        {
            List<RebarHookType> rebarHookTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarHookType)).Cast<RebarHookType>()
                .ToList();

            foreach (RebarHookType rebarHookType in rebarHookTypes)
            {
                if (rebarHookType.Name == name)
                {
                    return rebarHookType;
                }
            }
            return null;
        }

        /// <summary>
        /// Создает продольную арматуру на основе curves и выбранной формы
        /// </summary>
        /// <returns>
        /// Созданный стержень
        /// </returns>
        private Rebar _creatureRebarFromCurvesAndShape(Document doc, RebarShape rebarShape, RebarBarType rebarBarType, Element element, List<Curve> rebarCurves)
        {
            XYZ mainRebarNormal = new XYZ(0, 1, 0);  //Нормаль для построения стержней основной арматуры
            Rebar rebar = Rebar.CreateFromCurvesAndShape(
                doc,
                rebarShape,
                rebarBarType,
                null,
                null,
                element,
                mainRebarNormal,
                rebarCurves,
                RebarHookOrientation.Right,
                RebarHookOrientation.Right);
            return rebar;
        }


        /// <summary>
        /// Создает хомуты на основе curves и выбранной формы, а также отгиба арматуры
        /// </summary>
        /// <returns>
        /// Созданный хомут
        /// </returns>
        private Rebar _creatureRebarClampsFromCurvesAndShape(Document doc, RebarShape rebarShape, RebarBarType rebarBarType, Element element, List<Curve> rebarCurves)
        {
            XYZ clampRebarNormal = new XYZ(0, 0, 1);  //Нормаль для построения стержней основной арматуры
            Rebar rebar = Rebar.CreateFromCurvesAndShape(
                doc,
                rebarShape,
                rebarBarType,
                rebarHookType,
                rebarHookType,
                element,
                clampRebarNormal,
                rebarCurves,
                RebarHookOrientation.Right,
                RebarHookOrientation.Right);
            return rebar;
            //doc.GetElement(rebarShape.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId());
        }
    }
}
