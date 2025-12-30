using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AddressBook.Data;
using AddressBook.Models;

namespace AddressBook
{
    public partial class MainWindow : Window
    {
        private readonly PersonRepository _personRepo = new();
        private readonly AddressRepository _addressRepo = new();

        // ListBox'a bunu basacağız (adres sayısı dahil)
        private sealed class PersonRow
        {
            public Person Person { get; set; } = null!;
            public string FullName => Person.FullName;
            public string? Phone => Person.Phone;
            public int AddressCount { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            RefreshPersons();
        }

        // -----------------------------
        // LIST / SEARCH
        // -----------------------------
        private void RefreshPersons_Click(object sender, RoutedEventArgs e) => RefreshPersons();

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => RefreshPersons();

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            RefreshPersons();
        }

        private void RefreshPersons()
        {
            var people = _personRepo.GetAll();

            var q = (SearchBox?.Text ?? "").Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(q))
            {
                people = people
                    .Where(p =>
                        (p.FullName ?? "").ToLowerInvariant().Contains(q) ||
                        (p.Phone ?? "").ToLowerInvariant().Contains(q))
                    .ToList();
            }

            // AddressCount hesapla
            var rows = new List<PersonRow>();
            foreach (var p in people)
            {
                var count = _addressRepo.GetByPersonId(p.Id).Count;
                rows.Add(new PersonRow { Person = p, AddressCount = count });
            }

            PersonsList.ItemsSource = rows;
        }

        // -----------------------------
        // TOP BAR
        // -----------------------------
        private void CloseApp_Click(object sender, RoutedEventArgs e) => Close();

        // -----------------------------
        // ADD PERSON (ana ekrandan)
        // -----------------------------
        private void NewPerson_Click(object sender, RoutedEventArgs e)
        {
            var dialog = BuildPersonCreateDialog();
            dialog.Owner = this;

            var ok = dialog.ShowDialog();
            if (ok == true) RefreshPersons();
        }

        // -----------------------------
        // ROW ACTIONS (👁 ✏ 🗑)
        // -----------------------------
        private void ViewPerson_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is not Person p) return;

            var dialog = BuildPersonViewDialog(p.Id);
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void EditPerson_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is not Person p) return;

            var dialog = BuildPersonEditDialog(p.Id);
            dialog.Owner = this;

            var ok = dialog.ShowDialog();
            if (ok == true) RefreshPersons();
        }

        private void DeletePersonRow_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is not Person p) return;

            var ok = MessageBox.Show(
                $"{p.FullName} kişisi silinsin mi?\n\n(Not: Kişiye bağlı adresler de silinir.)",
                "Silme Onayı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (ok != MessageBoxResult.Yes) return;

            // Önce adresleri sil (FK varsa sorun olmasın)
            var addrs = _addressRepo.GetByPersonId(p.Id);
            foreach (var a in addrs)
                _addressRepo.Delete(a.Id);

            _personRepo.Delete(p.Id);
            RefreshPersons();
        }

        // -----------------------------
        // THEME HELPERS (dialoglar da aynı pastel görünümde olsun)
        // -----------------------------
        private void ApplyTheme(Window win)
        {
            // MainWindow stilleri/dialog’a taşınsın
            win.Resources = this.Resources;

            // Arka plan (Bg varsa onu kullan)
            if (this.Resources.Contains("Bg"))
                win.Background = (Brush)this.Resources["Bg"];
            else
                win.Background = Brushes.White;
        }

        private UIElement WrapCard(UIElement content)
        {
            var bg = this.Resources.Contains("Surface") ? (Brush)this.Resources["Surface"] : Brushes.White;
            var br = this.Resources.Contains("Border") ? (Brush)this.Resources["Border"] : Brushes.LightGray;

            return new Border
            {
                Background = bg,
                BorderBrush = br,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(16),
                Margin = new Thickness(16),
                Child = content
            };
        }

        // -----------------------------
        // CREATE DIALOG (Yeni Kişi)
        // -----------------------------
        private Window BuildPersonCreateDialog()
        {
            var win = new Window
            {
                Title = "Yeni Kişi",
                Width = 560,
                Height = 340,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize
            };
            ApplyTheme(win);

            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var title = new TextBlock
            {
                Text = "Yeni Kişi Ekle",
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            root.Children.Add(title);

            var form = new Grid();
            form.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            form.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            form.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var fn = new TextBox();
            var ln = new TextBox();
            var ph = new TextBox();

            form.Children.Add(MakeField("Ad", fn, 0));
            form.Children.Add(MakeField("Soyad", ln, 1));
            form.Children.Add(MakeField("Telefon", ph, 2));

            Grid.SetRow(form, 2);
            root.Children.Add(form);

            var btnRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var cancel = new Button { Content = "İptal", Width = 130, Height = 40, Margin = new Thickness(0, 0, 8, 0) };
            var save = new Button { Content = "Kaydet", Width = 130, Height = 40 };

            cancel.Click += (_, __) => win.DialogResult = false;
            save.Click += (_, __) =>
            {
                var first = (fn.Text ?? "").Trim();
                var last = (ln.Text ?? "").Trim();
                var phone = (ph.Text ?? "").Trim();

                if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
                {
                    MessageBox.Show("Ad ve Soyad boş olamaz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _personRepo.Add(new Person
                {
                    FirstName = first,
                    LastName = last,
                    Phone = string.IsNullOrWhiteSpace(phone) ? null : phone
                });

                win.DialogResult = true;
            };

            btnRow.Children.Add(cancel);
            btnRow.Children.Add(save);

            Grid.SetRow(btnRow, 4);
            root.Children.Add(btnRow);

            win.Content = WrapCard(root);
            return win;
        }

        private static UIElement MakeField(string label, Control input, int col)
        {
            var sp = new StackPanel { Margin = new Thickness(0, 0, col == 2 ? 0 : 10, 0) };
            sp.Children.Add(new TextBlock { Text = label, Margin = new Thickness(0, 0, 0, 6), Opacity = 0.75 });
            sp.Children.Add(input);
            Grid.SetColumn(sp, col);
            return sp;
        }

        // -----------------------------
        // VIEW DIALOG (READ ONLY)
        // -----------------------------
        private Window BuildPersonViewDialog(int personId)
        {
            var person = _personRepo.GetAll().First(x => x.Id == personId);
            var addresses = _addressRepo.GetByPersonId(personId);

            var win = new Window
            {
                Title = "Kişi Görüntüle",
                Width = 760,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize
            };
            ApplyTheme(win);

            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var header = new StackPanel();
            header.Children.Add(new TextBlock { Text = person.FullName, FontSize = 18, FontWeight = FontWeights.SemiBold });
            header.Children.Add(new TextBlock { Text = person.Phone ?? "", Opacity = 0.7, Margin = new Thickness(0, 4, 0, 0) });
            header.Children.Add(new TextBlock { Text = $"Adresler ({addresses.Count})", Opacity = 0.75, Margin = new Thickness(0, 8, 0, 0) });
            root.Children.Add(header);

            var list = new ListBox { ItemsSource = addresses };
            list.ItemTemplate = BuildAddressReadTemplate();
            Grid.SetRow(list, 2);
            root.Children.Add(list);

            var close = new Button { Content = "Kapat", Width = 130, Height = 40, HorizontalAlignment = HorizontalAlignment.Right };
            close.Click += (_, __) => win.DialogResult = false;

            Grid.SetRow(close, 3);
            root.Children.Add(close);

            win.Content = WrapCard(root);
            return win;
        }

        private static DataTemplate BuildAddressReadTemplate()
        {
            var dt = new DataTemplate(typeof(Address));

            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(10));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            border.SetValue(Border.BorderBrushProperty, Brushes.LightGray);
            border.SetValue(Border.PaddingProperty, new Thickness(10));
            border.SetValue(Border.MarginProperty, new Thickness(0, 0, 0, 8));

            var sp = new FrameworkElementFactory(typeof(StackPanel));

            var t1 = new FrameworkElementFactory(typeof(TextBlock));
            t1.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Title"));
            t1.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);

            var t2 = new FrameworkElementFactory(typeof(TextBlock));
            t2.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("DisplayText"));
            t2.SetValue(TextBlock.OpacityProperty, 0.8);
            t2.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);

            sp.AppendChild(t1);
            sp.AppendChild(t2);
            border.AppendChild(sp);

            dt.VisualTree = border;
            return dt;
        }

        // ✅ Edit penceresinde de AddressBook.Models.Address yazmasın diye ayrı template
        private static DataTemplate BuildAddressEditTemplate()
        {
            var dt = new DataTemplate(typeof(Address));

            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(10));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            border.SetValue(Border.BorderBrushProperty, Brushes.LightGray);
            border.SetValue(Border.PaddingProperty, new Thickness(10));
            border.SetValue(Border.MarginProperty, new Thickness(0, 0, 0, 8));

            var sp = new FrameworkElementFactory(typeof(StackPanel));

            var t1 = new FrameworkElementFactory(typeof(TextBlock));
            t1.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Title"));
            t1.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);

            var t2 = new FrameworkElementFactory(typeof(TextBlock));
            t2.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("DisplayText"));
            t2.SetValue(TextBlock.OpacityProperty, 0.8);
            t2.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);

            sp.AppendChild(t1);
            sp.AppendChild(t2);
            border.AppendChild(sp);

            dt.VisualTree = border;
            return dt;
        }

        // -----------------------------
        // EDIT DIALOG (PERSON + ADDRESS CRUD)
        // -----------------------------
        private Window BuildPersonEditDialog(int personId)
        {
            var person = _personRepo.GetAll().First(x => x.Id == personId);

            var win = new Window
            {
                Title = "Kişi Düzenle",
                Width = 920,
                Height = 720,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize
            };
            ApplyTheme(win);

            // state
            Address? editingAddress = null;

            // root
            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // title
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // person form
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // addresses
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // buttons

            // title
            var title = new TextBlock
            {
                Text = "Düzenleme",
                FontSize = 18,
                FontWeight = FontWeights.SemiBold
            };
            root.Children.Add(title);

            // person form
            var personForm = new Grid();
            personForm.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            personForm.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            personForm.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var tbFn = new TextBox { Text = person.FirstName };
            var tbLn = new TextBox { Text = person.LastName };
            var tbPh = new TextBox { Text = person.Phone ?? "" };

            personForm.Children.Add(MakeField("Ad", tbFn, 0));
            personForm.Children.Add(MakeField("Soyad", tbLn, 1));
            personForm.Children.Add(MakeField("Telefon", tbPh, 2));

            Grid.SetRow(personForm, 2);
            root.Children.Add(personForm);

            // addresses area
            var addrGrid = new Grid();
            addrGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            addrGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
            addrGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // left list
            var addrList = new ListBox();
            addrList.ItemTemplate = BuildAddressEditTemplate(); // ✅ artık düzgün görünecek

            // ✅ TextBox'ları ÖNCE oluştur
            var tbATitle = new TextBox();
            var tbACity = new TextBox();
            var tbADistrict = new TextBox();
            var tbALine = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Height = 110,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            Action refreshAddrList = () =>
            {
                addrList.ItemsSource = _addressRepo.GetByPersonId(person.Id);
            };
            refreshAddrList();

            addrList.SelectionChanged += (_, __) =>
            {
                if (addrList.SelectedItem is not Address a) return;
                editingAddress = a;

                tbATitle.Text = a.Title ?? "";
                tbACity.Text = a.City ?? "";
                tbADistrict.Text = a.District ?? "";
                tbALine.Text = a.AddressLine ?? "";
            };

            addrGrid.Children.Add(addrList);
            Grid.SetColumn(addrList, 0);

            // right edit panel
            var right = new StackPanel();

            right.Children.Add(new TextBlock { Text = "Adres Başlığı (Ev/İş)", Opacity = 0.75, Margin = new Thickness(0, 0, 0, 6) });
            right.Children.Add(tbATitle);

            right.Children.Add(new TextBlock { Text = "İl", Opacity = 0.75, Margin = new Thickness(0, 10, 0, 6) });
            right.Children.Add(tbACity);

            right.Children.Add(new TextBlock { Text = "İlçe", Opacity = 0.75, Margin = new Thickness(0, 10, 0, 6) });
            right.Children.Add(tbADistrict);

            right.Children.Add(new TextBlock { Text = "Adres", Opacity = 0.75, Margin = new Thickness(0, 10, 0, 6) });
            right.Children.Add(tbALine);

            var addrBtnRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0)
            };

            var btnNewAddr = new Button { Content = "Yeni Adres", Width = 130, Height = 40, Margin = new Thickness(0, 0, 8, 0) };
            var btnSaveAddr = new Button { Content = "Kaydet/Güncelle", Width = 160, Height = 40, Margin = new Thickness(0, 0, 8, 0) };
            var btnDelAddr = new Button { Content = "Sil", Width = 110, Height = 40 };

            void ClearAddrForm()
            {
                editingAddress = null;
                addrList.SelectedItem = null;
                tbATitle.Clear();
                tbACity.Clear();
                tbADistrict.Clear();
                tbALine.Clear();
            }

            btnNewAddr.Click += (_, __) => ClearAddrForm();

            btnSaveAddr.Click += (_, __) =>
            {
                var titleAddr = (tbATitle.Text ?? "").Trim();
                var city = (tbACity.Text ?? "").Trim();
                var dist = (tbADistrict.Text ?? "").Trim();
                var line = (tbALine.Text ?? "").Trim();

                if (string.IsNullOrWhiteSpace(line))
                {
                    MessageBox.Show("Adres alanı boş olamaz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (editingAddress == null)
                {
                    _addressRepo.Add(new Address
                    {
                        PersonId = person.Id,
                        Title = string.IsNullOrWhiteSpace(titleAddr) ? null : titleAddr,
                        City = string.IsNullOrWhiteSpace(city) ? null : city,
                        District = string.IsNullOrWhiteSpace(dist) ? null : dist,
                        AddressLine = line
                    });
                }
                else
                {
                    editingAddress.Title = string.IsNullOrWhiteSpace(titleAddr) ? null : titleAddr;
                    editingAddress.City = string.IsNullOrWhiteSpace(city) ? null : city;
                    editingAddress.District = string.IsNullOrWhiteSpace(dist) ? null : dist;
                    editingAddress.AddressLine = line;

                    _addressRepo.Update(editingAddress);
                }

                refreshAddrList();
                ClearAddrForm();
            };

            btnDelAddr.Click += (_, __) =>
            {
                if (editingAddress == null)
                {
                    MessageBox.Show("Silmek için bir adres seç.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var ok = MessageBox.Show("Seçili adres silinsin mi?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (ok != MessageBoxResult.Yes) return;

                _addressRepo.Delete(editingAddress.Id);
                refreshAddrList();
                ClearAddrForm();
            };

            addrBtnRow.Children.Add(btnNewAddr);
            addrBtnRow.Children.Add(btnSaveAddr);
            addrBtnRow.Children.Add(btnDelAddr);

            right.Children.Add(addrBtnRow);

            Grid.SetColumn(right, 2);
            addrGrid.Children.Add(right);

            Grid.SetRow(addrGrid, 4);
            root.Children.Add(addrGrid);

            // bottom buttons (Kişi kaydet / iptal)
            var bottom = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var cancel = new Button { Content = "İptal", Width = 130, Height = 40, Margin = new Thickness(0, 0, 8, 0) };
            var save = new Button { Content = "Kişiyi Kaydet", Width = 150, Height = 40 };

            cancel.Click += (_, __) => win.DialogResult = false;

            save.Click += (_, __) =>
            {
                var first = (tbFn.Text ?? "").Trim();
                var last = (tbLn.Text ?? "").Trim();
                var phone = (tbPh.Text ?? "").Trim();

                if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
                {
                    MessageBox.Show("Ad ve Soyad boş olamaz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                person.FirstName = first;
                person.LastName = last;
                person.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone;

                _personRepo.Update(person);

                win.DialogResult = true;
            };

            bottom.Children.Add(cancel);
            bottom.Children.Add(save);

            Grid.SetRow(bottom, 5);
            root.Children.Add(bottom);

            ClearAddrForm();

            win.Content = WrapCard(root);
            return win;
        }
    }
}
