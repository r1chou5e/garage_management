﻿using DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace GarageManagement.User_Control
{
    public partial class CheckoutForm : UserControl
    {
        private DataTable listCar = null;
        private DataRow currentCar = null;
        private double totalCostOfSquarePart = 0; // tổng chi phí phụ tùng
        private double totalRepairCost = 0; // tổng chi phí sữa chữa
        public CheckoutForm()
        {
            InitializeComponent();
        }

        private void CheckoutForm_Load(object sender, EventArgs e)
        {
            resetAllValue();
            loadCarComboBox();
        }

        private void loadCarComboBox() {
            carComboBox.Items.Clear();
            DataTable listCar = XE_DAL.Instance.LoadCarListOnStatus(2);
            this.listCar = listCar;
            for (int i = 0; i < listCar.Rows.Count; i++) {
                carComboBox.Items.Add(listCar.Rows[i]["BienSo"].ToString() + " | " + listCar.Rows[i]["TenHieuXe"].ToString());
            }
            if (carComboBox.Items.Count > 0) carComboBox.SelectedIndex = 0;
        }
        private void checkCheckoutCondition() {
            if (confirmCheckbox.Checked == true && carComboBox.Text != "" && paymentMethodComboBox.Text != "")
            {
                submitBtn.Enabled = true;
            }
            else
            {
                submitBtn.Enabled = false;
            }
        }
        private void checkCustomberPanelCondition() {
            if (this.currentCar != null && customerPanel.Visible == false) {
                customerPanel.Visible = true;
            }
        }
        private void clearAllItemsInProblemListView() {
            ProblemListView.Items.Clear();
        }
        private void clearAllItemsInOrderListView()
        {
            orderDetailListView.Items.Clear();
        }
        private void resetAllValue() {
            currentCar = null;
            totalCostOfSquarePart = 0;
            totalRepairCost = 0;
            totalCostLabel.Text = "";
            totalCostOfSquarePartLabel.Text = "";
            totalRepairCostLabel.Text = "";
            taxLabel.Text = "";
            customerLabel.Text = "";
            addressLabel.Text = "";
            carPlateLabel.Text = "";
            carBrandLabel.Text = "";
            receivedDayLabel.Text = "";
            carComboBox.SelectedIndex = -1;
            paymentMethodComboBox.SelectedIndex = -1;
            confirmCheckbox.Checked = false;
            ProblemListView.Items.Clear();
            orderDetailListView.Items.Clear();
        } 
        private void carComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string value = (string)carComboBox.SelectedItem;
            if (carComboBox.SelectedIndex == -1) return;
            int index = carComboBox.SelectedIndex > 0 ? carComboBox.SelectedIndex : 0;
            this.currentCar = this.listCar.Rows[index];

            checkCheckoutCondition();
            checkCustomberPanelCondition();

            DataTable customerInfo = KHACHHANG_DAL.Instance.GetCustomerById(this.listCar.Rows[index]["MaKH"].ToString());
            if (customerInfo.Rows.Count == 0) {
                MessageBox.Show("Không tìm thấy khách hàng");
                return;
            }
            customerLabel.Text = customerInfo.Rows[0]["TenKH"].ToString();
            addressLabel.Text = customerInfo.Rows[0]["DiaChi"].ToString();
            carPlateLabel.Text = this.listCar.Rows[index]["BienSo"].ToString();
            carBrandLabel.Text = this.listCar.Rows[index]["TenHieuXe"].ToString();
            receivedDayLabel.Text = this.listCar.Rows[index]["NgayTiepNhan"].ToString();

            // get repair card
            DataTable repairCards = PHIEUSUACHUA_DAL.Instance.GetRepairCardFromCarNotInPHIEUTHUTIEN(int.Parse(this.currentCar["MaXe"].ToString()));
            if (repairCards.Rows.Count == 0) {
                MessageBox.Show("Không tìm thấy phiếu sữa chữa");
                return;
            }
            // get list problem
            DataTable problem = CHITIETTIENCONG_DAL.Instance.LoadProblemDetail(int.Parse(repairCards.Rows[0]["MaPhieuSuaChua"].ToString()));
            clearAllItemsInProblemListView();
            int i = 1;
            foreach (DataRow row in problem.Rows) {
                ListViewItem lvItem = new ListViewItem(i + "");
                lvItem.SubItems.Add(row["TenTienCong"].ToString());
                lvItem.SubItems.Add(row["ChiPhi"].ToString());
                ProblemListView.Items.Add(lvItem);
                // add cost of square part item to totalRepairCost
                totalRepairCost += double.Parse(row["ChiPhi"].ToString());
                i++;
            }

            // get list order detail
            DataTable orderDetail = CHITIETPHIEUSUACHUA_DAL.Instance.GetKitListOfCar(int.Parse(repairCards.Rows[0]["MaPhieuSuaChua"].ToString()));
            clearAllItemsInOrderListView();
            i = 1;
            foreach (DataRow row in orderDetail.Rows)
            {
                ListViewItem lvItem = new ListViewItem(i + "");
                lvItem.SubItems.Add(row["TenVatTuPhuTung"].ToString());
                lvItem.SubItems.Add(row["DonGia"].ToString());
                lvItem.SubItems.Add(row["SoLuongPhuTung"].ToString());
                orderDetailListView.Items.Add(lvItem);
                // add repair cost item to totalCostOfSquarePart
                totalCostOfSquarePart += double.Parse(row["DonGia"].ToString()) * double.Parse(row["SoLuongPhuTung"].ToString());
                i++;
            }

            loadDataToTotalPanel();
        }
        private void loadDataToTotalPanel() {
            totalCostOfSquarePartLabel.Text = FormatMoney((int)totalCostOfSquarePart) + " VND";
            totalRepairCostLabel.Text = FormatMoney((int)totalRepairCost) + " VND";
            totalCostLabel.Text = FormatMoney((int)((totalCostOfSquarePart + totalRepairCost) * 1.1)) + " VND";
            taxLabel.Text = "10.00%";
        }
        private void confirmCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            checkCheckoutCondition();
        }
        private void paymentMethodComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkCheckoutCondition();
        }
       
        private void submitBtn_Click(object sender, EventArgs e)
        {
            PHIEUTHUTIEN_DAL.Instance.addBill(int.Parse(this.currentCar["MaXe"].ToString()),double.Parse(((totalCostOfSquarePart + totalRepairCost) * 0.9).ToString()), int.Parse(paymentMethodComboBox.SelectedIndex.ToString()), DateTime.Now);
            XE_DAL.Instance.UpdateCarStatus(int.Parse(this.currentCar["MaXe"].ToString()), 3);
            MessageBox.Show("Thanh toán thành công");
            resetAllValue();
            loadCarComboBox();
        }

        string FormatMoney(int money)
        {
            List<string> all = new List<string>();
            while (money != 0)
            {
                int remainder = (money % 1000);
                string part = "";
                if (remainder < 10)
                {
                    part = "00" + remainder;
                }
                else if (remainder < 100)
                {
                    part = "0" + remainder;
                }
                else if (remainder < 1000)
                {
                    part = "" + remainder;
                }
                all.Add(part);
                money /= 1000;
            }
            string formatedMoney = "";
            all[all.Count - 1] = (Int32.Parse(all[all.Count - 1])).ToString();
            for (int i = all.Count - 1; i >= 0; i--)
            {
                if (i == 0)
                {
                    formatedMoney += all[i];
                }
                else
                {
                    formatedMoney += all[i] + ".";
                }
            }
            return formatedMoney;
        }
    }
}
