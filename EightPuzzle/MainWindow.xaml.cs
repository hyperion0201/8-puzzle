using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;


namespace EightPuzzle {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        readonly DispatcherTimer timeTracker;
        TimeSpan time;
        public int timeRemain = 180;
        string filename = "";
        List<Image> pieces = new List<Image>();
       
        List<Image> originPieces = new List<Image>();
        private Image draggedImage;
        private Point originPostion;
        

        public MainWindow() {
            InitializeComponent();
            time = TimeSpan.FromSeconds(1);
            timeTracker = new DispatcherTimer {
                Interval = time
            };
            timeTracker.Tick += new EventHandler(timer_Tick);
            //timeTracker.Start();
        }
        private void timer_Tick(object sender, EventArgs e) {
            timer.Text = ParseSecondToTime(timeRemain);
            if (timeRemain == 0) timeTracker.Stop();
            timeRemain--;
        }
        private string ParseSecondToTime(int second) {
            int h = second / 3600;
            int m = Math.Abs((second % 3600) / 60);
            int s = second - (h * 3600) - (m * 60);
            return $"{h}:0{m}:{s}";
        }

        private bool CheckWin(List<Image> current) {
            int failedCount = 0;
            for (int i = 0; i < current.Count; i++) {
                var tag = current[i].Tag as Tuple<int, int, int,int,int>;
                if (tag.Item3 != (tag.Item2 * 3 + tag.Item1)) failedCount++;
            }
            if (failedCount  > 0) return false;
            return true;
        }
        private void Init(object sender, RoutedEventArgs e) {
            var screen = new OpenFileDialog();
            if (screen.ShowDialog() == true) {
                filename = screen.FileName;
                var image = new BitmapImage(
                    new Uri(screen.FileName)
                    );
                originImage.Source = image;

                // begin split image
                var width = image.PixelWidth / 3;
                var height = image.PixelHeight / 3;
                var newHeight = 99 * height / width;
                var imgName = 0;
                for (int i = 0; i < 3; i++) {
                    for (int j = 0; j < 3; j++) {
                        if (i == 2 && j == 2) continue; 
                        var cropped = new CroppedBitmap(image,
                            new Int32Rect(i * width, j * height, width, height));

                        var imgPiece = new Image() {
                            Name = $"image{imgName}",
                            Source = cropped,
                            Width = 99,
                            Height = 99,
                            Tag = new Tuple<int, int, int, int, int>(i, j, -1, i, j) // (originX, originY, currPos, curX, curY)
                        };

                        pieces.Add(imgPiece); // store current piece for play
                      

                    }
                }

                var indices = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
                var rng = new Random();

                for (int i = 0; i < 3; i++) {
                    for (int j = 0; j < 3; j++) {
                        if (!(i == 2 && j == 2)) {
                            int index = rng.Next(indices.Count);

                            var img = pieces[indices[index]];
                            
                            //img.MouseLeftButtonDown += Img_MouseLeftButtonDown;

                            gameRegion.Children.Add(img);

                            var tag = img.Tag as Tuple<int, int, int,int,int>;
                            img.Tag = new Tuple<int, int, int,int,int>
                                (tag.Item1, tag.Item2, j * 3 + i, i,j);

                            Canvas.SetLeft(img, i * (100) );
                            Canvas.SetTop(img, j * (100 + 1) );

                            indices.RemoveAt(index);
                        }
                    }
                }

                for (int i=0;i<pieces.Count;i++) {
                    var tag = pieces[i].Tag as Tuple<int, int, int, int, int>;
                    if (tag.Item3==-1) {
                        pieces.Remove(pieces[i]);
                    }
                }
            }
        }

        private void Img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var img = sender as Image;
        // MessageBox.Show(img.Tag.ToString());
        }
        private void GameRegion_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var image = e.Source as Image;
            if (image != null && gameRegion.CaptureMouse()) {
                // handle if current image can be moved ?
               
                originPostion = e.GetPosition(gameRegion);
                draggedImage = image;

                Panel.SetZIndex(draggedImage, 1);
                
            }
        }

       private bool IsTargetBlank(int x, int y) {
            for (int i =0;i<pieces.Count;i++) {
                var tag = pieces[i].Tag as Tuple<int, int, int, int, int>;
                if (tag.Item4 == x && tag.Item5 == y) return false;
            }
            return true;
        }

        private bool UpElementBlank(int x, int y) {
            if ((y - 1) < 0) return false;
            for (int i = 0; i < pieces.Count; i++) {
                var tag = pieces[i].Tag as Tuple<int, int, int, int, int>;
                if (tag.Item4==x && tag.Item5 == (y - 1))
                    // found element, return 
                    return false;
            }
            return true;
        }
        private bool DownElementBlank(int x, int y) {
            if ((y + 1) > 2) return false;
            for (int i=0;i<pieces.Count;i++) {
                var tag = pieces[i].Tag as Tuple<int, int, int, int, int>;
                if (tag.Item4 == x && tag.Item5 == (y + 1)) return false;
            }
            return true;
        }

        private bool LeftElementBlank(int x, int y) {
            if ((x-1)<0) return false;
            for (int i =0;i<pieces.Count;i++) {
                var tag = pieces[i].Tag as Tuple<int, int, int, int, int>;
                if (tag.Item4 == (x - 1) && tag.Item5 == y) return false;
            }
            return true;
        }

        private bool RightElementBlank(int x, int y) {
            if ((x+1)>2) return false;
            for (int i =0;i<pieces.Count;i++) {
                var tag = pieces[i].Tag as Tuple<int, int, int, int, int>;
                if (tag.Item4 == (x + 1) && tag.Item5 == y) return false;
            }
            return true;
        }
        private void GameRegion_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (draggedImage != null) {
                gameRegion.ReleaseMouseCapture();
                  var currentPosition = e.GetPosition(this);
                var nearX = (int)((Canvas.GetLeft(draggedImage) + 50) / 100);
                var nearY = (int)((Canvas.GetTop(draggedImage) + 50) / 100);
                var curTag = draggedImage.Tag as Tuple<int, int, int, int, int>;

                // handle if (target is farther 1 cell than current
                if (Math.Abs((nearX-curTag.Item4))+Math.Abs(nearY-curTag.Item5)>1) {
                    
                    // set draggedImage to previous position (
                    Canvas.SetLeft(draggedImage, curTag.Item4 * 100);
                    Canvas.SetTop(draggedImage, curTag.Item5 * 100);
                    //   MessageBox.Show($"{nearX}/{nearY}");
                    Panel.SetZIndex(draggedImage, 0);
                    draggedImage = null;
                    return;
                }
                
                // detect if target (nearX, nearY) is a image or not
                if (IsTargetBlank(nearX,nearY)) {

                    

                    for (int j = 0; j < pieces.Count; j++) {
                        if (pieces[j].Tag as Tuple<int, int, int, int, int> == curTag) {
                            var tagFound = pieces[j].Tag as Tuple<int, int, int, int, int>;
                            // set new position
                            pieces[j].Tag = new Tuple<int, int, int, int, int>(tagFound.Item1, tagFound.Item2, nearY * 3 + nearX, nearX, nearY);

                        }
                    }

                    //Title = $"{nearX}/{nearY}";
                    Canvas.SetLeft(draggedImage, nearX * 100);
                    Canvas.SetTop(draggedImage, nearY * 100);
                    //   MessageBox.Show($"{nearX}/{nearY}");
                    Panel.SetZIndex(draggedImage, 0);
                    draggedImage = null;
                    if (CheckWin(pieces)) {
                        MessageBox.Show("You are done.");
                    }
                }
                else {
                    // set draggedImage to previous position (
                    Canvas.SetLeft(draggedImage, curTag.Item4 * 100);
                    Canvas.SetTop(draggedImage, curTag.Item5 * 100);
                    //   MessageBox.Show($"{nearX}/{nearY}");
                    Panel.SetZIndex(draggedImage, 0);
                    draggedImage = null;
                }
                

            }
            
        } 

        private void GameRegion_MouseMove(object sender, MouseEventArgs e) {
            if (draggedImage!=null) {

                // get origin postion
                
                var position = e.GetPosition(gameRegion);
               
                Debug.WriteLine(position.ToString());
                
                var offset = position - originPostion;
                originPostion = position;
                double newLeft = Canvas.GetLeft(draggedImage) + offset.X;
                double newTop = Canvas.GetTop(draggedImage) + offset.Y;
                
                // prevent from dragging outside canvas
                if (newLeft < 0) newLeft = 0;
                else if (newLeft + draggedImage.ActualWidth > 300)
                    newLeft = 300 - draggedImage.ActualWidth;

                if (newTop < 0) newTop = 0;
                else if (newTop + draggedImage.ActualHeight > 300)
                    newTop = 300 - draggedImage.ActualHeight;
                Title = $"{Canvas.GetLeft(draggedImage)+50}/{Canvas.GetTop(draggedImage)+50}";
                Canvas.SetLeft(draggedImage, newLeft);
                Canvas.SetTop(draggedImage, newTop);
              
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            var writer = new StreamWriter("save.txt");
           
            writer.WriteLine(filename);

            foreach (var img in pieces) {
                var tag = img.Tag as Tuple<int, int, int,int,int>;

                writer.WriteLine(tag.ToString());
            }

            writer.Close();
           
        }

        private void MoveUp(object sender, RoutedEventArgs e) {
           // find element in pieces which can move down, mean tag.Item5 - 1 is empty
           for (int i =0;i<pieces.Count;i++) {
                var tag = pieces[i].Tag as Tuple<int, int, int, int, int>;
                if (UpElementBlank(tag.Item4,tag.Item5)==true) {
                    // perform move piece[i];
                    Canvas.SetLeft(pieces[i], tag.Item4 * 100);
                    Canvas.SetTop(pieces[i], (tag.Item5 - 1) * 100);
                    // set new tuple
                    pieces[i].Tag = new Tuple<int, int, int, int, int>(tag.Item1, tag.Item2, tag.Item3-3, tag.Item4, tag.Item5 - 1);
                    return;
                }
            }
        }

        private void MoveDown(object sender, RoutedEventArgs e) {
            // find element in pieces which can move down, mean tag.Item5 + 1 is empty
            for (int i =0;i<pieces.Count;i++) {
                var tag = pieces[i].Tag as Tuple<int, int, int, int, int>;
                if (DownElementBlank(tag.Item4, tag.Item5)==true) {
                    Canvas.SetLeft(pieces[i], tag.Item4 * 100);
                    Canvas.SetTop(pieces[i], (tag.Item5 + 1) * 100);

                    // set new tuple
                    pieces[i].Tag = new Tuple<int, int, int, int, int>(tag.Item1, tag.Item2, tag.Item3+3, tag.Item4, tag.Item5 + 1);
                    return;
                }
            }
        }

        private void MoveLeft(object sender, RoutedEventArgs e) {
            // find element in pieces which can move left, mean tag.Item4 - 1 is empty
            for (int i =0;i<pieces.Count;i++) {
                var tag = pieces[i].Tag as Tuple<int, int, int, int, int>;
                if(LeftElementBlank(tag.Item4, tag.Item5)==true) {
                    Canvas.SetLeft(pieces[i], (tag.Item4 - 1) * 100);
                    Canvas.SetTop(pieces[i], tag.Item5 * 100);

                    // set new tuple
                    pieces[i].Tag = new Tuple<int, int, int, int, int>(tag.Item1, tag.Item2, tag.Item3-1, tag.Item4 - 1, tag.Item5);
                    return;
                }
            }
        }

        private void MoveRight(object sender, RoutedEventArgs e) {
            // find element in pieces which can move left, mean tag.Item4 - 1 is empty
            for (int i = 0; i < pieces.Count; i++) {
                var tag = pieces[i].Tag as Tuple<int, int, int, int, int>;
                if (RightElementBlank(tag.Item4, tag.Item5) == true) {
                    Canvas.SetLeft(pieces[i], (tag.Item4 + 1) * 100);
                    Canvas.SetTop(pieces[i], tag.Item5 * 100);

                    // set new tuple
                    pieces[i].Tag = new Tuple<int, int, int, int, int>(tag.Item1, tag.Item2, tag.Item3+1, tag.Item4 + 1, tag.Item5);
                    return;
                }
            }
        }
    }
}

// 