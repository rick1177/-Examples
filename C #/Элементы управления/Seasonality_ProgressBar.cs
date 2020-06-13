using EgoldsUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using System.IO;

namespace Seasonality_for_project.Controls
{
    class Seasonality_ProgressBar : Control
    {

        #region --События--
        public delegate void OnValueChangedEvent(int value);
        public event OnValueChangedEvent OnValueChanged;

        #endregion

        #region --Переменные--

        //переменные для отслеживания нажатия
        Stopwatch st = new Stopwatch();
        MouseButtons mb = MouseButtons.None;

        int rectProgressCentralLineFinishPosition=0,
            rectProgressCentralLinePosition=0;
        int rectProgressCentralLineStartPosition = 0;
        private int x = 0;
        private int y = 0;
        System.Timers.Timer timer = new System.Timers.Timer(500);

        #endregion

        #region --Свойства--

        public int OnTimedEventValue { get; private set; }

        //свойства оформления
        public Color BorderColor { get; set; } = FlatColors.GrayDark;
        public override Color BackColor { get; set; } = FlatColors.GrayLight;
        public Color BackColorProgress_1 { get; set; } = Color.FromArgb(150, 255, 99, 71);
        public Color BackColorProgress_2 { get; set; } = Color.FromArgb(255, 255, 99, 71);

        //УРОВЕНЬ ГРАФИКА     

        private float _temp1 = 0, _temp2=0;

        private int _value = 50;
        private int _valueMinimum = -100;
        private int _valueMaximum = 100;
        private int _valueMiddle = 0;

        public int Value
        {
            get => _value;
            set
            {
                if (value >= ValueMinimum && value <= ValueMaximum)
                {
                    _value = value;
                    Invalidate();
                }
                else
                {
                    value = _value;
                    Invalidate();
                }
                OnValueChanged?.Invoke(_value);
            }
        }
        public int ValueMinimum
        {
            get => _valueMinimum;
            set
            {
                if (value < _valueMaximum)
                {
                    _temp1 = _valueMinimum;
                    _valueMinimum = value;
                    
                    /*_valueMiddle =
                        (_valueMiddle - _temp1) /
                        (_valueMaximum - _temp1) *
                        (_valueMaximum - _valueMinimum) + _valueMinimum;*/
                    _temp1 = 0;

                    if (_value < _valueMinimum)
                    {
                        _value = _valueMinimum;
                    }

                    Invalidate();
                }
                else
                {
                    value = _valueMinimum;
                    Invalidate();
                }
                //OnValueChanged?.Invoke(_value);
            }
        }
        public int ValueMaximum
        {
            get => _valueMaximum;
            set
            {
                if (value > _valueMinimum)
                {
                    _temp1 = _valueMaximum;
                    _valueMaximum = value;
                    /*_valueMiddle =
                        (_valueMiddle - _valueMinimum) /
                        (_temp1 - _valueMinimum) *
                        (_valueMaximum - _valueMinimum) + _valueMinimum;*/
                    _temp1 = 0;
                    _temp2 = 0;

                    if (_value > _valueMaximum)
                    {
                        _value = _valueMaximum;
                    }
                    Invalidate();
                }
                else
                {
                    value = _valueMaximum;
                    Invalidate();
                }
                //OnValueChanged?.Invoke(_value);
            }
        }
        public int ValueMiddle
        {
            get => _valueMiddle;
            set
            {
                if (value >= ValueMinimum && value <= ValueMaximum)
                {
                    _valueMiddle = value;
                    Invalidate();
                }
                else
                {
                    value = _valueMiddle;
                    Invalidate();
                }
                //OnValueChanged?.Invoke(_value);
            }
        }

        #endregion

        public Seasonality_ProgressBar()
        {
            //зададим базовые настройки для устранения мерцания
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.UserPaint, true);
            DoubleBuffered = true;

            Size = new System.Drawing.Size(20, 100);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            //данный объект класса будет полностью отвечать за визуализацию
            Graphics graph = e.Graphics;
            //устанавливаем отрисовку высокого качества
            graph.SmoothingMode = SmoothingMode.HighQuality;

            graph.SmoothingMode = SmoothingMode.AntiAlias;

            //очистка холста
            graph.Clear(Parent.BackColor);


            Rectangle rectBase = new Rectangle(0, 0, Width-1, Height-1);

            //Rectangle rectBase2 = new Rectangle(1, 1, Width - 3, Height - 3);
            //Rectangle rectProgress = new Rectangle((rectBase.X+1), (rectBase.Y+ rectBase.Height/2), rectBase.Width - 2, 10);


            rectProgressCentralLineStartPosition = 
                (int)Math.Round((double)(ValueMiddle - ValueMinimum) /
                        (ValueMaximum - ValueMinimum) * (Height)) + rectBase.Y;

            rectProgressCentralLinePosition = rectProgressCentralLineStartPosition;

            double A = 
                    (double)(Value - ValueMiddle) / (ValueMaximum - ValueMinimum)  *Height;

            int rectProgressHeight= (int)Math.Round(Math.Abs(A));
            
            rectProgressCentralLineFinishPosition = rectProgressCentralLineStartPosition + rectProgressHeight;

            if (Math.Sign(Value - ValueMiddle) <= 0)
            {
                rectProgressCentralLineStartPosition -= rectProgressHeight;
                rectProgressCentralLineFinishPosition -= rectProgressHeight;
            }

            
            Rectangle rectProgressCentralLine = 
                new Rectangle(rectBase.X, (int)rectProgressCentralLinePosition, 
                rectBase.Width, 1);

            /*float StartPoint(double Value) => Value < ValueMiddle  ? 
                (float)(ValueMinimum + ValueMiddle - Value) :
                ValueMiddle;*/
            
            Rectangle rectProgress = 
                new Rectangle(rectBase.X,
                (int)(rectProgressCentralLineStartPosition),
                 rectBase.Width, (int)rectProgressHeight);

            /*     new PointF(rectBase.X + 1, 
             rectProgressCentralLineStartPosition),
             new SizeF(rectBase.Width - 1, (int)rectProgressHeight));*/

            //Определяем градиент


            //Прорисовывается основа
            DrawBase(graph, rectBase, rectBase);
            //Определяем градиент           
            if (Math.Abs(Value-ValueMiddle) >1)
            {
                LinearGradientBrush LGB = new LinearGradientBrush(
                    rectProgress, BackColorProgress_1, BackColorProgress_2, 90* Math.Sign(Value-ValueMiddle));
                DrawProgress(LGB, graph, rectProgress);
            }
            DrawProgress(new SolidBrush(FlatColors.GrayDark), graph, rectProgressCentralLine);

        }


        #region -- Рисование объектов --
        private void DrawProgress(Brush brush, Graphics graph, Rectangle rect)
        {

            //Прорисовка заливки прямоугольника самого Progress
            graph.FillRectangle(brush, rect);

        }
        private void DrawBase(Graphics graph, Rectangle rect, Rectangle rect2)
        {
            //Прорисовка заливки прямоугольника
            graph.FillRectangle(new SolidBrush(BackColor), rect);
            //Прорисовка обводки прямоугольника
            graph.DrawRectangle(new Pen(FlatColors.GrayDark), rect);
        }
        #endregion

        #region -- Управление --


        protected override void OnMouseDown(MouseEventArgs e)        {
            base.OnMouseDown(e);
            // base.OnMouseDown(e);
            if (!st.IsRunning)
            {
                mb = e.Button;
                st.Start();

                if (e.Button == mb && x != e.X && y != e.Y)
                {                    
                    x = e.X;
                    y = e.Y;
                    timer.Elapsed += OnTimedEvent;
                    timer.Start();
                    //Value = Value;
                    //File.AppendAllText("log.txt", "OnMouseDown\r\n");
                }
            }         
        }


        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            timer.Stop();
            //MessageBox.Show("Сработало");
            float reultation = rectProgressCentralLinePosition - y;
            if (reultation > 0)
            {
                Value = (int)(reultation / rectProgressCentralLinePosition * ValueMinimum);
                if (Value > 100) { Value = 100; }
            }
            else
            {
                Value = -(int)(reultation / rectProgressCentralLinePosition * ValueMaximum);
                if (Value < -100) { Value = -100; }
            }
            OnTimedEventValue = Value;
            //File.AppendAllText("log.txt", "OnTimedEvent\r\n");
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (st.ElapsedMilliseconds > 1000 &&  e.Button == MouseButtons.Left) // или любую другую, какая удобнее
            {
                timer.Stop();
                float reultation = rectProgressCentralLinePosition - e.Location.Y;
                if (reultation > 0)
                {
                    Value = (int)(reultation / rectProgressCentralLinePosition * ValueMinimum);
                    if (Value > 100) { Value = 100; }
                }
                else
                {
                    Value = -(int)(reultation / rectProgressCentralLinePosition * ValueMaximum);
                    if (Value < -100) { Value = -100; }
                }
                //File.AppendAllText("log.txt", "OnMouseMove\r\n");
            }            
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == mb)
            {
                if (st.ElapsedMilliseconds > 1000 && e.Button == mb)
                {
                    st.Stop();
                    //MessageBox.Show("Вы держали нажатой " + e.Button.ToString() + " кнопку мыши более 2x секунд, а именно: " + st.Elapsed.ToString());
                }
                else
                {
                    timer.Stop();

                    if (e.Location.Y < rectProgressCentralLineFinishPosition &&
                        Value > ValueMinimum)
                    {
                        if (Value < ValueMiddle && e.Location.Y > rectProgressCentralLineStartPosition)
                        {
                            Value += 1;
                        }
                        else { Value -= 1; }
                    }

                        if (e.Location.Y > rectProgressCentralLineFinishPosition &&
                        Value < ValueMaximum) { Value += 1; }
                }

                if (Value > 100) { Value = 100; }
                if (Value < -100) { Value = -100; }

                //MessageBox.Show("Координаты Y " + e.Location.Y);
                mb = MouseButtons.None;
                st.Reset();
                //File.AppendAllText("log.txt", "OnMouseUp\r\n");
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            float reultation = rectProgressCentralLinePosition - e.Location.Y;
            if (reultation > 0)
            {
                Value = (int)(reultation / rectProgressCentralLinePosition * ValueMinimum);
                if (Value > 100) { Value = 100; }
            }
            else
            {
                Value = -(int)(reultation / rectProgressCentralLinePosition * ValueMaximum);
                if (Value < -100) { Value = -100; }
            }
            //File.AppendAllText("log.txt", "OnMouseDoubleClick\r\n");

        }
        #endregion

    }
}
