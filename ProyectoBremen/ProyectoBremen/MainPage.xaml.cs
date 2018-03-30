using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0xc0a

namespace ProyectoBremen
{
    /// <summary>
    /// Página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += (s, a) =>
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                    a.Handled = true;
                }
            };
        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PerfilViga pe = new PerfilViga(2);
            Modulo mo = new Modulo(23000000);
            FuerzaYDistancia fu = new FuerzaYDistancia(500.0, 2.0);
            FuerzaYDistancia fu2 = new FuerzaYDistancia(501.0, 3.0);
            List<FuerzaYDistancia> fus = new List<FuerzaYDistancia>();
            fus.Add(fu);
            fus.Add(fu2);
            CalculadorPorTablas calc = new CalculadorPorTablas(pe,mo,fus,"a");
        }

        private void Frame_Loaded(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(PaginaInicio));

        }
    }
}
