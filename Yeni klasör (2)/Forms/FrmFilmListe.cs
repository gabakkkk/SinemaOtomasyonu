using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraGrid.Columns;
using DevExpress.Data;
using sinema.Models;
using DevExpress.XtraEditors;
namespace sinema.Forms
{
    public partial class FrmFilmListe : DevExpress.XtraEditors.XtraForm
    {
        // Veritabanı bağlantısı tüm metotların dışında olmalı
        SinemaOtomasyonDbEntities db = new SinemaOtomasyonDbEntities();

        public FrmFilmListe()
        {
            InitializeComponent();
            Listele();
            ResimSutunuHazirla();
        }

        void Listele()
        {
            // DB context'i yenileyerek veritabanından güncel veriyi çekelim
            db = new SinemaOtomasyonDbEntities();
            gridControl1.DataSource = db.TblFilmler.ToList();

            // GridView'i verileri yeniden çizmesi için tetikleyelim
            gridControl1.RefreshDataSource();
        }
        void ResimSutunuHazirla()
        {
            if (tileView1.Columns["SanalResim"] == null)
            {
                TileViewColumn colResim = new TileViewColumn();
                colResim.FieldName = "SanalResim";
                colResim.UnboundType = UnboundColumnType.Object;
                colResim.Visible = false;
                tileView1.Columns.Add(colResim);
                tileView1.ColumnSet.BackgroundImageColumn = colResim;
            }
            tileView1.CustomUnboundColumnData += TileView1_CustomUnboundColumnData;
        }

        private void TileView1_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
        {
            if (e.Column.FieldName == "SanalResim" && e.IsGetData)
            {
                string dosyaYolu = Convert.ToString(tileView1.GetListSourceRowCellValue(e.ListSourceRowIndex, "AfisYolu"));
                if (!string.IsNullOrEmpty(dosyaYolu) && System.IO.File.Exists(dosyaYolu))
                {
                    try { e.Value = Image.FromFile(dosyaYolu); } catch { e.Value = null; }
                }
            }
        }
        private void tileView1_ItemClick(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemClickEventArgs e)
        {
            // Hata: image_edec29.png'deki RowHandle tanımı hatası için düzeltme:
            var secilenFilm = tileView1.GetRow(e.Item.RowHandle) as TblFilmler;

            if (secilenFilm != null)
            {
                FrmFilmDetay frm = new FrmFilmDetay();
                frm.gelenFilmId = secilenFilm.ID;
                frm.Show();
            }
        }
        // SİLME METODU
        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var secilenFilm = tileView1.GetFocusedRow() as TblFilmler;
            if (secilenFilm == null) return;

            if (MessageBox.Show(secilenFilm.FilmAdi + " ve bağlı tüm kayıtlar silinsin mi?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    int fId = secilenFilm.ID;

                    // 1. Önce bu filme ait biletleri (seanslar üzerinden) temizle
                    var seanslar = db.TblSeanslar.Where(x => x.FilmID == fId).ToList();
                    foreach (var s in seanslar)
                    {
                        var biletler = db.TblBiletler.Where(x => x.SeansID == s.ID).ToList();
                        foreach (var b in biletler) db.TblBiletler.Remove(b);
                        db.TblSeanslar.Remove(s); // Sonra seansı temizle
                    }

                    // 2. En son filmi temizle
                    var film = db.TblFilmler.Find(fId);
                    db.TblFilmler.Remove(film);

                    db.SaveChanges();
                    Listele();
                    MessageBox.Show("Film ve ilişkili tüm veriler başarıyla temizlendi.");
                }
                catch (Exception ex) { MessageBox.Show("Hata oluştu: " + ex.Message); }
            }
        }

        private void FrmFilmListe_Activated(object sender, EventArgs e)
        {
            Listele();
        }
     


        private void timer1_Tick(object sender, EventArgs e)
            {
                // tileView1 sizin Grid üzerindeki TileView'ınızın adıdır
                int currentRow = tileView1.FocusedRowHandle;
                int totalRows = tileView1.RowCount;


                // Eğer son filme gelindiyse başa dön, gelmediyse bir sonrakine geç
                if (currentRow < totalRows - 1)
                {
                    tileView1.FocusedRowHandle = currentRow + 1;
                }
                else
                {
                    tileView1.FocusedRowHandle = 0; // Başa dön
                }
            }

        private void gridControl1_MouseEnter(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private void gridControl1_MouseLeave(object sender, EventArgs e)
        {
            timer1.Start();
        }

        
            private void btnChatbotAc_Click(object sender, EventArgs e)
            {
                // Formun zaten açık olup olmadığını kontrol etmek iyi bir mühendislik yaklaşımıdır
                FrmChatbot frm = new FrmChatbot();
                frm.Show(); // ShowDialog() derseniz arkadaki formlara tıklayamazsınız, Show() daha iyidir.
            }
        
    }
}