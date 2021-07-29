using System;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.IO;
using System.Windows.Media.Imaging;
using StructuralStrengthening;
using System.Drawing;
using System.Drawing.Imaging;
using StructuralStrengthening.Properties;
using System.Windows.Media;
using ReinforcementSquareColumns;
using ReinforcementStructuresModel;
using ReinforcementSquareColumns.RequestHandler;

namespace MyRobot.ReinforcementSquareColumns
{
    public class Add : IExternalApplication
    {
        string RibbonTab = "MyRobot";
        string RibbonPanel = "MyRebarPanel";
        public Result OnStartup(UIControlledApplication application)
        {

            try
            {
                application.CreateRibbonTab(RibbonTab);
            }
            catch (Exception) { }//tab adready exists*/
            // get or create the panet
            RibbonPanel ribbonPanel = null;
            foreach (RibbonPanel pnl in application.GetRibbonPanels(RibbonTab))
            {
                if (pnl.Name == RibbonPanel)
                {
                    ribbonPanel = pnl;
                    break;
                }
            }
            if (ribbonPanel == null)
            {
                ribbonPanel = application.CreateRibbonPanel(RibbonTab, RibbonPanel);
            }

            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData buttonData = new PushButtonData("cmdHelloWorld", "Арм. квад.\n колонны",
                thisAssemblyPath, "MyRobot.ReinforcementSquareColumns.Command");
            PushButton pushButton = ribbonPanel.AddItem(buttonData) as PushButton;

            // При желании кнопке могут быть присвоены другие свойства.
            // a) Подсказка
            pushButton.ToolTip = "Армирование квадратной колонны";

            // b) Растровое изображение

            //Uri uriImage = new Uri(@"C:\Users\PAGrabovskii\source\repos\StructuralStrengthening\StructuralStrengthening\Properties\ImageRebarSquareColumns.jpg");
            //pushButton.LargeImage = new BitmapImage(uriImage);


            Image ImageRebarSquareColumns = global::StructuralStrengthening.Properties.Resources.LogoRebar;
            ImageSource _imageSoursRebarSquareColumns = GetImageSourse(ImageRebarSquareColumns);
            pushButton.LargeImage = _imageSoursRebarSquareColumns;
            return Result.Succeeded;
        }

        private ImageSource GetImageSourse(Image img)
        {
            BitmapImage bmp = new BitmapImage();
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                ms.Position = 0;

                bmp.BeginInit();

                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = null;
                bmp.StreamSource = ms;

                bmp.EndInit();
            }
            return bmp;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // nothing to clean up in this simple case
            return Result.Succeeded;
        }
    }


    // Транзакция, обязательно для запуска команды!
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]

    public class Command : IExternalCommand
    {
        // The main Execute method (inherited from IExternalCommand) must be public
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            // A new handler to handle request posting by the dialog
            RequestHandlerCreateRebar handler = new RequestHandlerCreateRebar();

            // External Event for the dialog to use (to post requests)
            ExternalEvent exEvent = ExternalEvent.Create(handler);

            var view = new ReinforcementSquareColumnsView(exEvent, handler);

            var model = new ReinforcementStructures(revit);

            var presenter = new ReinforcementSquareColumnsPresenter(revit, view, model);
            //presenter(view, model)


            view.Show();

            return Autodesk.Revit.UI.Result.Succeeded;

        }
    }
}
