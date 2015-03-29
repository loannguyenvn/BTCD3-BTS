﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLTramBTS
{
    public delegate void MyDelegate(string s);

    public partial class ThongtinMaynoForm : Form
    {
        TramContext tramContext;
        GenericRepository<ChayMayNo> chayMayNoRepo;
        GenericRepository<Tram> tramRepo;
        String[] fieldName = { "Mã", "Ngày Giờ Cúp Điện", "Ngày Giờ Chạy Máy Nổ", "Số Giờ Chạy Máy Nổ", "Số Lần Vi Phạm", "Mã Trạm" };
        public static MyDelegate myDelegate;
        bool edit = false;

        public ThongtinMaynoForm()
        {
            InitializeComponent();
            tramContext = new TramContext();
            chayMayNoRepo = new GenericRepository<ChayMayNo>(tramContext);// chứa insert, update,delete
            tramRepo = new GenericRepository<Tram>(tramContext);

            dtpNgayGioMatDien.Format = DateTimePickerFormat.Custom;
            dtpNgayGioMatDien.CustomFormat = "dd/MM/yyyy - HH:mm:ss"; //HH 24h, hh 12h, tt = am/pm

            dtpNgayGioChayMayNo.Format = DateTimePickerFormat.Custom;
            dtpNgayGioChayMayNo.CustomFormat = "dd/MM/yyyy - HH:mm:ss"; 

            txtSoLanViPham.Text = "0";

            foreach (String s in fieldName)
            {
                listView.Columns.Add(s, 150, HorizontalAlignment.Left);
                //-2 : autosize the column to the length of the text in the column header
                //-1 : autosize to the longest item in the column
                cbSearch.Items.Add(s);
            }
            cbSearch.SelectedItem = cbSearch.Items[0];
        }

        public void Init()
        {
            var listTram = tramRepo.Get();
            cbTramId.Items.Clear();
            foreach (var tram in listTram)
            {
                cbTramId.Items.Add(tram.TramId);
            }
            UpdateList();
            btnReset_Click(null, null);
        }

        void UpdateList()
        {
            listView.Items.Clear();
            int i = 0;
            var list = chayMayNoRepo.Get();
            foreach (var chayMayNo in list)
            {
                ListViewItem item = new ListViewItem(chayMayNo.ChayMayNoId);
                item.SubItems.Add(chayMayNo.NgayGioMatDien.ToString());
                item.SubItems.Add(chayMayNo.NgayGioChayMayNo.ToString());
                item.SubItems.Add(chayMayNo.SoGioChayMayNo.ToString());
                item.SubItems.Add(chayMayNo.SoLanViPham.ToString());
                item.SubItems.Add(chayMayNo.TramId);
                listView.Items.Add(item);
            }
        }

        private void SetDataToObject(ChayMayNo chayMayNo)
        {
            chayMayNo.ChayMayNoId = txtMa.Text;
            chayMayNo.NgayGioMatDien = dtpNgayGioMatDien.Value;
            chayMayNo.NgayGioChayMayNo = dtpNgayGioChayMayNo.Value;
            chayMayNo.SoGioChayMayNo = Single.Parse(txtSoGioChay.Text);//, CultureInfo.InvariantCulture);
            chayMayNo.SoLanViPham = Int32.Parse(txtSoLanViPham.Text);
            chayMayNo.TramId = cbTramId.Text;
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (!CheckDataOnScreen())
            {
                return;
            }

            ChayMayNo chayMayNo = new ChayMayNo();
            SetDataToObject(chayMayNo);

            try
            {
                chayMayNoRepo.Insert(chayMayNo);
                chayMayNoRepo.Save();
                myDelegate.Invoke(chayMayNo.TramId);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToString(), "Error");
                chayMayNoRepo.Delete(chayMayNo);
                MessageBox.Show("Đã tồn tại Mã = \"" + chayMayNo.ChayMayNoId + "\"", "Error");
            }

            UpdateList();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection selectedList;
            selectedList = listView.SelectedItems;
            List<string> tramId = new List<string>();
            foreach (ListViewItem item in selectedList)
            {
                string id = item.SubItems[0].Text;
                ChayMayNo chayMayNo = chayMayNoRepo.GetByID(id);
                tramId.Add(chayMayNo.TramId);
                chayMayNoRepo.Delete(id);
            }
            chayMayNoRepo.Save();
            UpdateList();

            foreach (string s in tramId)
            {
                myDelegate.Invoke(s);
            }

            btnReset_Click(null, null);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtMa.Text = "";
            txtMa.Enabled = true;
            dtpNgayGioMatDien.ResetText();
            dtpNgayGioChayMayNo.ResetText();
            txtSoGioChay.Text = "";
            txtSoLanViPham.Text = "0";
            cbTramId.ResetText();
            btnThem.Enabled = true;
            btnSua.Enabled = false;
            btnXoa.Enabled = false;
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (!CheckDataOnScreen())
            {
                return;
            }

            ChayMayNo chayMayNo = chayMayNoRepo.GetByID(txtMa.Text);
            SetDataToObject(chayMayNo);
            try
            {
                chayMayNoRepo.Update(chayMayNo);
                chayMayNoRepo.Save();
                myDelegate.Invoke(chayMayNo.TramId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error");
            }

            UpdateList();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (txtSearch.Text.Length == 0)
            {
                MessageBox.Show("Nhập dữ liệu cần tìm vào textbox!", "Error");
                return;
            }
            int index = cbSearch.SelectedIndex;
            int resultCount = 0;
            foreach (ListViewItem item in listView.Items)
            {
                item.BackColor = Color.White;

                if (item.SubItems[index].Text.Contains(txtSearch.Text))
                {
                    item.BackColor = Color.Cyan;
                    resultCount++;
                }
            }
            MessageBox.Show(resultCount + " result(s) was found.", "Search Result");
        }

        private void dtpNgayGioChayMayNo_ValueChanged(object sender, EventArgs e)
        {
            System.TimeSpan diff = dtpNgayGioChayMayNo.Value - dtpNgayGioMatDien.Value;
            if (diff.TotalHours > 0)
            {
                txtSoLanViPham.Text = diff.TotalHours > 2 ? "1" : "0";
            }
        }

        private void listView_Click(object sender, EventArgs e)
        {
            ListViewItem item = listView.SelectedItems[0];
            txtMa.Text = item.SubItems[0].Text;
            txtMa.Enabled = false;
            dtpNgayGioMatDien.Value = DateTime.Parse(item.SubItems[1].Text);
            dtpNgayGioChayMayNo.Value = DateTime.Parse(item.SubItems[2].Text);
            txtSoGioChay.Text = item.SubItems[3].Text;
            txtSoLanViPham.Text = item.SubItems[4].Text;
            cbTramId.Text = item.SubItems[5].Text;
            btnThem.Enabled = false;
            btnSua.Enabled = true;
            btnXoa.Enabled = true;
        }

        private bool CheckDataOnScreen()
        {
            bool ok = true;
            if (txtMa.Text.Length == 0)
            {
                MessageBox.Show("Mã không được để trống!", "Error");
                ok = false;
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(txtMa.Text, @"[^a-zA-Z0-9]+$"))
            {
                MessageBox.Show("Mã không được chứa kí tự đặc biệt!", "Error");
                ok = false;
            }

            if (txtSoGioChay.Text.Length == 0)
            {
                MessageBox.Show("Số giờ chạy không được để trống!", "Error");
                ok = false;
            }
            else
            {
                try
                {
                    float x = Single.Parse(txtSoGioChay.Text);
                    if (x < 0)
                    {
                        MessageBox.Show("Nhập số giờ chạy > 0", "Error");
                        ok = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString(), "Error");
                    ok = false;
                }
            }

            if (cbTramId.Text.Length == 0)
            {
                MessageBox.Show("Mã trạm không được để trống!", "Error");
                ok = false;
            }

            System.TimeSpan diff = dtpNgayGioChayMayNo.Value - dtpNgayGioMatDien.Value;
            
            if (!(diff.TotalSeconds > -1))
            {
                MessageBox.Show("Giờ chạy máy nổ phải trễ hơn hoặc bằng giờ mất điện!", "Error");
                ok = false;
            }

            return ok;
        }
    }
}
