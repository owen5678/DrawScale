using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
//using System.IO;

namespace ScreenLine
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private LineGeometry g_lineGeometry;
        private Point g_startPoint;
        private SolidColorBrush Color;
        public enum ClickState
        {
            Initial = 0,
            Idle,
            FirstClick,
            Picture,
            Drop,
        }
        private ClickState g_State;

        double dLabelY;
        double dSpotX, dSpotY;

        double FirstXPos, FirstYPos, FirstArrowXPos, FirstArrowYPos;
        double FirstLabelXPos, FirstLabelYPos, FirstArrowLabelXPos, FirstArrowLabelYPos;
        double FirstEllipseXPos, FirstEllipseYPos, FirstArrowEllipseXPos, FirstArrowEllipseYPos;

        object MovingObject;

        System.Windows.Controls.Control LastControl;
        System.Windows.Shapes.Ellipse LastEllipse;

        public MainWindow()
        {
            InitializeComponent();

            g_State = ClickState.Initial;
            Color = new SolidColorBrush(Colors.Red);

            System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show("ESC : 取消 Ctrl+c : 關閉程式", "使用說明", System.Windows.Forms.MessageBoxButtons.OK);
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                g_State = ClickState.Idle;
            }

            DesigningCanvas.PreviewMouseMove += this.MouseMove;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (g_State == ClickState.Initial)
            {
                return;
            }
            Point point = e.GetPosition(DesigningCanvas);

            if (g_State == ClickState.Idle)
            {
                g_State = ClickState.FirstClick;
                FirstTrigger(point);
            }
            else if (g_State == ClickState.FirstClick)
            {
                if (null != g_lineGeometry)
                {
                    EndTrigger(point);
                }
                g_State = ClickState.Picture;
            }
        }

        private void FirstTrigger(Point point)
        {
            Ellipse startPoint = new Ellipse();
            startPoint.Width = 20;
            startPoint.Height = 20;
            startPoint.Fill = Color;

            startPoint.SetValue(Canvas.LeftProperty, (double)point.X - 10);
            startPoint.SetValue(Canvas.TopProperty, (double)point.Y - 10);

            DesigningCanvas.Children.Add(startPoint);
            LastEllipse = startPoint;
            g_lineGeometry = new LineGeometry();
            g_lineGeometry.StartPoint = point;
            g_lineGeometry.EndPoint = point;

            System.Windows.Shapes.Path myPath = new System.Windows.Shapes.Path();
            myPath.Stroke = Color;
            myPath.StrokeThickness = 4;
            myPath.Data = g_lineGeometry;

            g_startPoint = point;
            DesigningCanvas.Children.Add(myPath);
        }

        private void EndTrigger(Point endPoint)
        {
            // 實線結束點
            Ellipse endEllipse = new Ellipse();
            endEllipse.Width = 20;
            endEllipse.Height = 20;
            endEllipse.Fill = Color;
            endEllipse.SetValue(Canvas.LeftProperty, (double)endPoint.X - 10);
            endEllipse.SetValue(Canvas.TopProperty, (double)endPoint.Y - 10);

            DesigningCanvas.Children.Add(endEllipse);

            //計算虛線終點
            Point calcEndPoint = new Point();
            calcEndPoint.X = endPoint.X - g_startPoint.X + endPoint.X;
            calcEndPoint.Y = endPoint.Y - g_startPoint.Y + endPoint.Y;
            ////劃一條虛線
            LineGeometry lineGeometry = new LineGeometry();
            lineGeometry.StartPoint = endPoint;
            lineGeometry.EndPoint = calcEndPoint;
            System.Windows.Shapes.Path myPath = new System.Windows.Shapes.Path();
            myPath.Stroke = Color;
            myPath.StrokeThickness = 4;
            myPath.Data = lineGeometry;
            myPath.StrokeDashArray = new DoubleCollection() { 2, 3 };

            DesigningCanvas.Children.Add(myPath);

            //// 虛線結束點
            Ellipse endDottedEllipse = new Ellipse();
            endDottedEllipse.Width = 20;
            endDottedEllipse.Height = 20;
            endDottedEllipse.Fill = Color;
            endDottedEllipse.SetValue(Canvas.LeftProperty, (double)calcEndPoint.X - 10);
            endDottedEllipse.SetValue(Canvas.TopProperty, (double)calcEndPoint.Y - 10);

            DesigningCanvas.Children.Add(endDottedEllipse);

            dSpotX = (double)calcEndPoint.X - 10 - ((double)endPoint.X - 10);
            dSpotY = (double)calcEndPoint.Y - 10 - ((double)endPoint.Y - 10);

            // 橫軸線
            // 0% Line
            LineGeometry startlineGeometry = new LineGeometry();
            startlineGeometry.StartPoint = new Point(g_startPoint.X, g_startPoint.Y);
            startlineGeometry.EndPoint = new Point(calcEndPoint.X, g_startPoint.Y);
            System.Windows.Shapes.Path startLinePath = new System.Windows.Shapes.Path();
            startLinePath.Stroke = Color;
            startLinePath.StrokeThickness = 4;

            startLinePath.Data = startlineGeometry;
            DesigningCanvas.Children.Add(startLinePath);


            // 50% Line
            LineGeometry midLineGeometry = new LineGeometry();
            midLineGeometry.StartPoint = new Point(g_startPoint.X, (g_startPoint.Y + endPoint.Y) / 2);
            midLineGeometry.EndPoint = new Point(calcEndPoint.X, (g_startPoint.Y + endPoint.Y) / 2);
            System.Windows.Shapes.Path midLinePath = new System.Windows.Shapes.Path();
            midLinePath.Stroke = Color;
            midLinePath.StrokeThickness = 4;

            midLinePath.Data = midLineGeometry;
            DesigningCanvas.Children.Add(midLinePath);

            // 100% Line
            LineGeometry solidLineEndGeometry = new LineGeometry();
            solidLineEndGeometry.StartPoint = new Point(g_startPoint.X, endPoint.Y);
            solidLineEndGeometry.EndPoint = new Point(calcEndPoint.X, endPoint.Y);
            System.Windows.Shapes.Path solidEndPath = new System.Windows.Shapes.Path();
            solidEndPath.Stroke = Color;
            solidEndPath.StrokeThickness = 4;

            solidEndPath.Data = solidLineEndGeometry;
            DesigningCanvas.Children.Add(solidEndPath);
            // 200% Line
            LineGeometry dottedLineGeometry = new LineGeometry();
            dottedLineGeometry.StartPoint = new Point(g_startPoint.X, calcEndPoint.Y); ;
            dottedLineGeometry.EndPoint = new Point(calcEndPoint.X, calcEndPoint.Y); ;
            System.Windows.Shapes.Path dottedLinePath = new System.Windows.Shapes.Path();
            dottedLinePath.Stroke = Color;
            dottedLinePath.StrokeThickness = 4;

            dottedLinePath.Data = dottedLineGeometry;
            DesigningCanvas.Children.Add(dottedLinePath);
            // 標數字

            double labelXpoint = 0;
            if (g_startPoint.X > calcEndPoint.X)
            {
                labelXpoint = calcEndPoint.X;
            }
            else
            {
                labelXpoint = g_startPoint.X;
            }

            System.Windows.Controls.Label startLabel = new System.Windows.Controls.Label();
            startLabel.Content = @"000%";
            startLabel.Foreground = Color;
            startLabel.SetValue(Canvas.LeftProperty, labelXpoint - 100);
            startLabel.SetValue(Canvas.TopProperty, g_startPoint.Y - 10);
            startLabel.FontSize = 20;

            DesigningCanvas.Children.Add(startLabel);

            System.Windows.Controls.Label midLabel = new System.Windows.Controls.Label();
            midLabel.Content = @"050%";
            midLabel.Foreground = Color;
            midLabel.SetValue(Canvas.LeftProperty, labelXpoint - 100);
            midLabel.SetValue(Canvas.TopProperty, (g_startPoint.Y + endPoint.Y) / 2 - 10);
            midLabel.FontSize = 20;
            DesigningCanvas.Children.Add(midLabel);

            dLabelY = ((g_startPoint.Y + endPoint.Y) / 2 - 10) - (g_startPoint.Y - 10);

            System.Windows.Controls.Label endSolidLineLabel = new System.Windows.Controls.Label();
            endSolidLineLabel.Content = @"100%";
            endSolidLineLabel.Foreground = Color;
            endSolidLineLabel.SetValue(Canvas.LeftProperty, labelXpoint - 100);
            endSolidLineLabel.SetValue(Canvas.TopProperty, endPoint.Y - 10);
            endSolidLineLabel.FontSize = 20;
            DesigningCanvas.Children.Add(endSolidLineLabel);

            System.Windows.Controls.Label endDottedLineLabel = new System.Windows.Controls.Label();
            endDottedLineLabel.Content = @"200%";
            endDottedLineLabel.Foreground = Color;
            endDottedLineLabel.SetValue(Canvas.LeftProperty, labelXpoint - 100);
            endDottedLineLabel.SetValue(Canvas.TopProperty, calcEndPoint.Y - 10);
            endDottedLineLabel.FontSize = 20;
            DesigningCanvas.Children.Add(endDottedLineLabel);

            try
            {
                foreach (object control in DesigningCanvas.Children)
                {
                    if (control.GetType() != typeof(System.Windows.Controls.Label))
                    {
                        ((Shape)control).PreviewMouseLeftButtonDown += this.MouseLeftButtonDown;
                        ((Shape)control).PreviewMouseLeftButtonUp += this.PreviewMouseLeftButtonUp;
                        ((Shape)control).Cursor = System.Windows.Input.Cursors.Hand;
                    }
                    if (control.GetType() == typeof(System.Windows.Controls.Label))
                    {
                        if (null == LastControl)
                        {
                            LastControl = (System.Windows.Controls.Control)control;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //string a = ex.Message;
            }

            g_lineGeometry = null;
        }




        private void OnKeyDownHandler(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DesigningCanvas.Children.Clear();
                g_lineGeometry = null;
                g_State = ClickState.Idle;
                LastControl = null;
                LastEllipse = null;
            }

            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

        void PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MovingObject = null;
        }

        private void MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            switch (g_State)
            {
                case ClickState.Idle:
                    break;
                case ClickState.FirstClick:
                    if (null == g_lineGeometry)
                    {
                        return;
                    }
                    Point punto = e.GetPosition(DesigningCanvas);
                    int mouseX = (int)punto.X;
                    int mouseY = (int)punto.Y;
                    g_lineGeometry.EndPoint = punto;
                    break;
                case ClickState.Picture:
                    if (e.LeftButton == MouseButtonState.Pressed && null != MovingObject)
                    {
                        int i = 0;
                        int j = 0;
                        foreach (object control in DesigningCanvas.Children)
                        {
                            if (control.GetType() == typeof(System.Windows.Shapes.Path))
                            {
                                (control as FrameworkElement).SetValue(Canvas.LeftProperty,
                                   e.GetPosition((control as FrameworkElement).Parent as FrameworkElement).X - FirstXPos);

                                (control as FrameworkElement).SetValue(Canvas.TopProperty,
                                    e.GetPosition((control as FrameworkElement).Parent as FrameworkElement).Y - FirstYPos);
                            }
                            else if (control.GetType() == typeof(System.Windows.Shapes.Ellipse))
                            {
                                (control as FrameworkElement).SetValue(Canvas.LeftProperty,
                                  e.GetPosition((control as FrameworkElement).Parent as FrameworkElement).X - FirstEllipseXPos + j * dSpotX);

                                (control as FrameworkElement).SetValue(Canvas.TopProperty,
                                    e.GetPosition((control as FrameworkElement).Parent as FrameworkElement).Y - FirstEllipseYPos + j * dSpotY);
                                j++;
                            }
                            else if (control.GetType() == typeof(System.Windows.Controls.Label))
                            {
                                if (3 == i)
                                {
                                    i++;
                                }
                                (control as FrameworkElement).SetValue(Canvas.LeftProperty,
                                   e.GetPosition((control as FrameworkElement).Parent as FrameworkElement).X - FirstLabelXPos);
                                (control as FrameworkElement).SetValue(Canvas.TopProperty,
                                    e.GetPosition((control as FrameworkElement).Parent as FrameworkElement).Y - FirstLabelYPos + i * dLabelY);
                                i++;
                            }
                        }
                    }
                    break;
            }
        }

        private void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //In this event, we get current mouse position on the control to use it in the MouseMove event.
            FirstXPos = e.GetPosition(sender as Shape).X;
            FirstYPos = e.GetPosition(sender as Shape).Y;
            FirstArrowXPos = e.GetPosition((sender as Shape).Parent as Shape).X - FirstXPos;
            FirstArrowYPos = e.GetPosition((sender as Shape).Parent as Shape).Y - FirstYPos;

            FirstLabelXPos = e.GetPosition(LastControl).X;
            FirstLabelYPos = e.GetPosition(LastControl).Y;
            FirstArrowLabelXPos = e.GetPosition((LastControl).Parent as System.Windows.Controls.Control).X - FirstLabelXPos;
            FirstArrowLabelYPos = e.GetPosition((LastControl).Parent as System.Windows.Controls.Control).Y - FirstLabelYPos;

            FirstEllipseXPos = e.GetPosition(LastEllipse).X;
            FirstEllipseYPos = e.GetPosition(LastEllipse).Y;
            FirstArrowEllipseXPos = e.GetPosition((LastEllipse).Parent as System.Windows.Controls.Control).X - FirstEllipseXPos;
            FirstArrowEllipseYPos = e.GetPosition((LastEllipse).Parent as System.Windows.Controls.Control).Y - FirstEllipseYPos;



            MovingObject = sender;
        }

    }


}
