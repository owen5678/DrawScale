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
        #region Customer requirement
        private readonly SolidColorBrush NONE_LINE_COLOR = new SolidColorBrush(Color.FromArgb(0xFF, 0xE3, 0xE3, 0xE3));
        private readonly SolidColorBrush HALF_LINE_COLOR = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xB0));
        private readonly SolidColorBrush ONE_LINE_COLOR = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x8C, 0x55));
        private readonly SolidColorBrush DOUBLE_LINE_COLOR = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x4D, 0xA6));
        private readonly SolidColorBrush SLOPE_LINE_COLOR = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
        private readonly double LINE_THICKNESS = 3;
        #endregion

        #region Inner Parameter
        private LineGeometry g_lineGeometry;
        private Point g_startPoint;

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
        //double dLineY;

        double FirstXPos, FirstYPos, FirstArrowXPos, FirstArrowYPos;
        double FirstLabelXPos, FirstLabelYPos, FirstArrowLabelXPos, FirstArrowLabelYPos;
        double FirstEllipseXPos, FirstEllipseYPos, FirstArrowEllipseXPos, FirstArrowEllipseYPos;

        object MovingObject;

        System.Windows.Controls.Control LastControl;
        System.Windows.Shapes.Ellipse LastEllipse;
        System.Windows.Shapes.Path StartPath;
        #endregion


        public MainWindow()
        {
            InitializeComponent();

            g_State = ClickState.Initial;

            System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show("ESC : 關閉程式 \nSpace : 取消 ", "使用說明", System.Windows.Forms.MessageBoxButtons.OK);
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
            startPoint.Width = 10;
            startPoint.Height = 10;
            startPoint.Fill = NONE_LINE_COLOR;

            startPoint.SetValue(Canvas.LeftProperty, (double)point.X - 5);
            startPoint.SetValue(Canvas.TopProperty, (double)point.Y - 5);

            DesigningCanvas.Children.Add(startPoint);
            LastEllipse = startPoint;
            g_lineGeometry = new LineGeometry();
            g_lineGeometry.StartPoint = point;
            g_lineGeometry.EndPoint = point;

            System.Windows.Shapes.Path myPath = new System.Windows.Shapes.Path();
            myPath.Stroke = SLOPE_LINE_COLOR;
            myPath.StrokeThickness = LINE_THICKNESS;
            myPath.Data = g_lineGeometry;

            g_startPoint = point;
            DesigningCanvas.Children.Add(myPath);
        }

        private void EndTrigger(Point endPoint)
        {
            // 實線結束點 (100%)
            Ellipse endEllipse = new Ellipse();
            endEllipse.Width = 10;
            endEllipse.Height = 10;
            endEllipse.Fill = ONE_LINE_COLOR;
            endEllipse.SetValue(Canvas.LeftProperty, (double)endPoint.X - 5);
            endEllipse.SetValue(Canvas.TopProperty, (double)endPoint.Y - 5);

            DesigningCanvas.Children.Add(endEllipse);

            //計算虛線終點 
            Point calcEndPoint = new Point();
            calcEndPoint.X = endPoint.X - g_startPoint.X + endPoint.X;
            calcEndPoint.Y = endPoint.Y - g_startPoint.Y + endPoint.Y;
            ////劃一條虛斜線 
            LineGeometry lineGeometry = new LineGeometry();
            lineGeometry.StartPoint = endPoint;
            lineGeometry.EndPoint = calcEndPoint;
            System.Windows.Shapes.Path myPath = new System.Windows.Shapes.Path();
            myPath.Stroke = SLOPE_LINE_COLOR;
            myPath.StrokeThickness = LINE_THICKNESS;
            myPath.Data = lineGeometry;
            myPath.StrokeDashArray = new DoubleCollection() { 2, 3 };

            DesigningCanvas.Children.Add(myPath);

            //// 虛線結束點 (200%)
            Ellipse endDottedEllipse = new Ellipse();
            endDottedEllipse.Width = 10;
            endDottedEllipse.Height = 10;
            endDottedEllipse.Fill = DOUBLE_LINE_COLOR;
            endDottedEllipse.SetValue(Canvas.LeftProperty, (double)calcEndPoint.X - 5);
            endDottedEllipse.SetValue(Canvas.TopProperty, (double)calcEndPoint.Y - 5);

            DesigningCanvas.Children.Add(endDottedEllipse);

            dSpotX = (double)calcEndPoint.X - 10 - ((double)endPoint.X - 10);
            dSpotY = (double)calcEndPoint.Y - 10 - ((double)endPoint.Y - 10);

            // 橫軸線
            // 0% Line
            LineGeometry startlineGeometry = new LineGeometry();
            startlineGeometry.StartPoint = new Point(g_startPoint.X, g_startPoint.Y);
            startlineGeometry.EndPoint = new Point(calcEndPoint.X, g_startPoint.Y);
            System.Windows.Shapes.Path startLinePath = new System.Windows.Shapes.Path();
            startLinePath.Stroke = NONE_LINE_COLOR;
            startLinePath.StrokeThickness = LINE_THICKNESS;

            startLinePath.Data = startlineGeometry;
            DesigningCanvas.Children.Add(startLinePath);
            StartPath = startLinePath;

            // 50% Line
            LineGeometry midLineGeometry = new LineGeometry();
            midLineGeometry.StartPoint = new Point(g_startPoint.X, (g_startPoint.Y + endPoint.Y) / 2);
            midLineGeometry.EndPoint = new Point(calcEndPoint.X, (g_startPoint.Y + endPoint.Y) / 2);
            System.Windows.Shapes.Path midLinePath = new System.Windows.Shapes.Path();
            midLinePath.Stroke = HALF_LINE_COLOR;
            midLinePath.StrokeThickness = LINE_THICKNESS;

            midLinePath.Data = midLineGeometry;
            DesigningCanvas.Children.Add(midLinePath);

            //dLineY = (g_startPoint.Y + endPoint.Y) / 2 - g_startPoint.Y;

            // 100% Line
            LineGeometry solidLineEndGeometry = new LineGeometry();
            solidLineEndGeometry.StartPoint = new Point(g_startPoint.X, endPoint.Y);
            solidLineEndGeometry.EndPoint = new Point(calcEndPoint.X, endPoint.Y);
            System.Windows.Shapes.Path solidEndPath = new System.Windows.Shapes.Path();
            solidEndPath.Stroke = ONE_LINE_COLOR;
            solidEndPath.StrokeThickness = LINE_THICKNESS;

            solidEndPath.Data = solidLineEndGeometry;
            DesigningCanvas.Children.Add(solidEndPath);
            // 200% Line
            LineGeometry dottedLineGeometry = new LineGeometry();
            dottedLineGeometry.StartPoint = new Point(g_startPoint.X, calcEndPoint.Y); ;
            dottedLineGeometry.EndPoint = new Point(calcEndPoint.X, calcEndPoint.Y); ;
            System.Windows.Shapes.Path dottedLinePath = new System.Windows.Shapes.Path();
            dottedLinePath.Stroke = DOUBLE_LINE_COLOR;
            dottedLinePath.StrokeThickness = LINE_THICKNESS;

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
            startLabel.Content = @"0%";
            startLabel.Foreground = NONE_LINE_COLOR;
            startLabel.SetValue(Canvas.LeftProperty, labelXpoint - 50);
            startLabel.SetValue(Canvas.TopProperty, g_startPoint.Y - 10);
            startLabel.FontSize = 10;

            DesigningCanvas.Children.Add(startLabel);

            System.Windows.Controls.Label midLabel = new System.Windows.Controls.Label();
            midLabel.Content = @"50%";
            midLabel.Foreground = HALF_LINE_COLOR;
            midLabel.SetValue(Canvas.LeftProperty, labelXpoint - 50);
            midLabel.SetValue(Canvas.TopProperty, (g_startPoint.Y + endPoint.Y) / 2 - 10);
            midLabel.FontSize = 10;
            DesigningCanvas.Children.Add(midLabel);

            dLabelY = ((g_startPoint.Y + endPoint.Y) / 2 - 10) - (g_startPoint.Y - 10);

            System.Windows.Controls.Label endSolidLineLabel = new System.Windows.Controls.Label();
            endSolidLineLabel.Content = @"100%";
            endSolidLineLabel.Foreground = ONE_LINE_COLOR;
            endSolidLineLabel.SetValue(Canvas.LeftProperty, labelXpoint - 50);
            endSolidLineLabel.SetValue(Canvas.TopProperty, endPoint.Y - 10);
            endSolidLineLabel.FontSize = 10;
            DesigningCanvas.Children.Add(endSolidLineLabel);

            System.Windows.Controls.Label endDottedLineLabel = new System.Windows.Controls.Label();
            endDottedLineLabel.Content = @"200%";
            endDottedLineLabel.Foreground = DOUBLE_LINE_COLOR;
            endDottedLineLabel.SetValue(Canvas.LeftProperty, labelXpoint - 50);
            endDottedLineLabel.SetValue(Canvas.TopProperty, calcEndPoint.Y - 10);
            endDottedLineLabel.FontSize = 10;
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
                        ((System.Windows.Controls.Control)control).PreviewMouseLeftButtonDown += this.MouseLeftButtonDown;
                        ((System.Windows.Controls.Control)control).PreviewMouseLeftButtonUp += this.PreviewMouseLeftButtonUp;
                        ((System.Windows.Controls.Control)control).Cursor = System.Windows.Input.Cursors.Hand;

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
            if (e.Key == Key.Space)
            {
                DesigningCanvas.Children.Clear();
                g_lineGeometry = null;
                g_State = ClickState.Idle;
                LastControl = null;
                LastEllipse = null;
            }

            if (e.Key == Key.Escape)
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
                        int k = 0;
                        foreach (object control in DesigningCanvas.Children)
                        {
                            if (control.GetType() == typeof(System.Windows.Shapes.Path))
                            {
                                (control as FrameworkElement).SetValue(Canvas.LeftProperty,
                                   e.GetPosition((control as FrameworkElement).Parent as FrameworkElement).X - FirstXPos  );

                                (control as FrameworkElement).SetValue(Canvas.TopProperty,
                                    e.GetPosition((control as FrameworkElement).Parent as FrameworkElement).Y - FirstYPos );
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
            FirstXPos = e.GetPosition(StartPath as Shape).X;
            FirstYPos = e.GetPosition(StartPath as Shape).Y;
            FirstArrowXPos = e.GetPosition((StartPath as Shape).Parent as Shape).X - FirstXPos;
            FirstArrowYPos = e.GetPosition((StartPath as Shape).Parent as Shape).Y - FirstYPos;

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
