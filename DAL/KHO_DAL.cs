﻿using DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class KHO_DAL
    {
        private static KHO_DAL instance;

        public static KHO_DAL Instance {
            get { if (instance == null) { instance = new KHO_DAL(); } return KHO_DAL.instance; }
            private set { KHO_DAL.instance = value; }
        }

        // this function is use to get accessories in stock
        public List<KHO> getAccessoriesInStock()
        {
            List<KHO> accessories = new List<KHO>();
            string query = "SELECT * FROM dbo.KHO";
            DataTable data = DataProvider.Instance.ExecuteQuery(query);
            foreach (DataRow item in data.Rows)
            {
                KHO kho = new KHO(item);
                accessories.Add(kho);
            }
            return accessories;
        }

        public void addAccessory(int MaPhuTung, string TenVatTuPhuTung,
            int SoLuong, int DonGia)
        {
            string query = "INSERT dbo.KHO {MaPhuTung,TenVatTuPhuTung,SoLuong,DonGia} VALUES ("+ MaPhuTung+ ","+ TenVatTuPhuTung + "," + SoLuong + ","+ DonGia + ")";
            DataProvider.Instance.ExecuteNonQuery(query);
        }

        public void deleteAccessory(int MaPhuTung)
        {
            string query = "DELETE FROM dbo.KHO WHERE MaPhuTung=" + MaPhuTung;
            DataProvider.Instance.ExecuteNonQuery(query);
        }

        public void updateAccessory(int MaPhuTung, string TenVatTuPhuTung,
            int SoLuong, int DonGia)
        {
            string query = "UPDATE dbo.KHO SET TenVatTuPhuTung=" + TenVatTuPhuTung + ",SoLuong=" + SoLuong + ",DonGia=" + DonGia + "WHERE MaPhuTung="+ MaPhuTung;
            DataProvider.Instance.ExecuteNonQuery(query);
        }
    }
}