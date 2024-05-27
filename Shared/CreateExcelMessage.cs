using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class CreateExcelMessage
    {       
        public int FileId { get; set; }

        //burada exceli datasını mesaj ile göndermicez büyük bir datada bu yöntem mantıklı değil
        //bunun yerine worker service içerisinde bu exceli  olusturucaz.
    }
}
