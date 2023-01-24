using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Linq;
using System;

using ConsumerApp;
using DataEntity.Config;
using DataEntity.Model.Input;
using DataEntity.Model.Output;
using System.Windows.Input;

namespace ConsumerApp {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        Consumption c = new Consumption();
      
        public MainWindow() {
            InitializeComponent();
            CMDListView.ItemsSource = null;
            CMDListView.ItemsSource = c.ShowCMD();
        }

        /// <summary>
        /// Start data consumption
        /// Change cmd status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConsumer_Click(object sender, RoutedEventArgs e) {
            c.StartThread();
            if (c.flag == 0) {
                MessageBox.Show("Datele au fost consumate!");
            }
        }

        /// <summary>
        /// Save data in cosumption tabel
        /// </summary>
        /// <param name="consumption"></param>
        /// <param name="bom"></param>
        /// <param name="productionOrder"></param>
        /// <param name="pailStatus"></param>
        /// <param name="pailCount"></param>
       
        /// <summary>
        /// Delete all data from consumption tabel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnResetConsumption_Click(object sender, RoutedEventArgs e) {
            using (var session = SqliteDB.Instance.GetSession()) {

                using (var transaction = session.BeginTransaction()) {
                    var consumer = session.Query<ProductionOrderConsumption>().ToList();
                    consumer.ForEach(item => {
                        session.Delete(item);
                    });
                    MessageBox.Show("Datele din tabela de consum au fost resetate!");
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Set "ELB" status for all cmd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnResetStatus_Click(object sender, RoutedEventArgs e) {
            using (var session = SqliteDB.Instance.GetSession()) {

                using (var transaction = session.BeginTransaction()) {
                    var productionOrder = session.Query<ProductionOrder>().ToList();
                    var pailStatus = session.Query<ProductionOrderPailStatus>().ToList();

                    productionOrder.ForEach(prodOrder => {
                        prodOrder.Status = "ELB";
                        session.Update(prodOrder);
                    });

                    pailStatus.ForEach(pail => {
                        pail.PailStatus = "ELB";
                        session.Update(pail);
                    });
                    MessageBox.Show("Datele comenzile au status ELB!");
                    transaction.Commit();
                }
            }
        }

        private void CMDListView_MouseLeftEvent(object sender, MouseButtonEventArgs e) {
            PailsListView.ItemsSource = null;
            System.Windows.Controls.ListView list = (System.Windows.Controls.ListView)sender;
            string selectedObject = (string)list.SelectedItem;
            if (selectedObject != null) {
                PailsListView.ItemsSource = c.ShowPails(selectedObject);
            }
        }

    }
}
