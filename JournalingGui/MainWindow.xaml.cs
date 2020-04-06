using System;
using System.Collections.Generic;
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
using Journaling;
namespace JournalingGui
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Dispatcher FilesDispatcher;
        public MainWindow()
        {
            InitializeComponent();
            FilesDispatcher = new Dispatcher();
            FilesGridUpdate();
        }
        public void FilesGridUpdate()
        {
            FilesList.Items.Clear();
            foreach(var VarFile in FilesDispatcher.Files)
            {

                FilesList.Items.Add(VarFile.Key);
            }
        }
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FilesDispatcher.Create(CreatedFileName.Text,5000);
                FilesGridUpdate();
            }
            catch
            {
                
                var res = MessageBox.Show($"Такой файл уже существует Создать файл {CreatedFileName.Text}", "Question", MessageBoxButton.YesNo);
                if (res==MessageBoxResult.Yes)
                {
                    FilesDispatcher.Create($"(Копия){CreatedFileName.Text}");
                }
                else
                {

                }
                FilesGridUpdate();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var y = FilesList.SelectedItem;
            FilesDispatcher.Delete(FilesList.SelectedItem.ToString(),5000);
            FilesGridUpdate();
        }

        private void FilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Contendtext.Text = FilesDispatcher.Read(FilesList.SelectedItem.ToString());

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            FilesDispatcher.Edit(FilesList.SelectedItem.ToString(), Contendtext.Text,3000);
        }
    }
}
