using System.ComponentModel.DataAnnotations.Schema;

namespace RabbitMqWeb.ExcelCreate.Models
{
    //file olusturulurken hangi asamada oldugunu belirtmek icin enum olusturduk
    public enum FileStatus
    {
        Creating,
        Completed
    }
    public class UserFile //olusturdugumuz excelleri db de  tutmak icin olusturulan model
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FileName { get; set; }
        public string? FilePath { get; set; }
        public DateTime? CreatedDate { get; set; } //daha sonra olusturulucagı için nullable yaptık
        public FileStatus FileStatus { get; set; }


        //veri tabanına kayıt edilmeyecek
        [NotMapped]

        //eğer createdate dolu ise valueyı alıyoruz boş ise - koyuyoruz
        public string GetCreatedDate => CreatedDate.HasValue ? CreatedDate.Value.ToShortDateString() :"-";
    }
}
