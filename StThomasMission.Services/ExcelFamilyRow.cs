using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class ExcelFamilyRow
    {
        public string? RegistrationNo { get; set; } // Column 1: Registration No(Auto)
        public string? FamilyName { get; set; } // Column 2: Family Name
        public string? WardName { get; set; } // Column 4: Family Unit (now ward name)
        public string? HouseNo { get; set; } // Column 18: House No
        public string? StreetName { get; set; } // Column 19: Street Name
        public string? City { get; set; } // Column 20: City
        public string? PostCode { get; set; } // Column 21: Post Code
        public string? Email { get; set; } // Column 22: E Mail
        public string? GiftAidStr { get; set; } // Column 97: Gift Aid
        public string? ParishIndia { get; set; } // Column 16: Parish India(Father)
        public string? EparchyIndia { get; set; } // Column 17: Eparchy in India(Father)

        // Father data
        public string? FatherFirstName { get; set; } // Column 7: First-Name(Father)
        public string? FatherSurname { get; set; } // Column 8: Surname(Father)
        public string? FatherBaptismalName { get; set; } // Column 9: Baptismal-Name(Father)
        public string? FatherContact { get; set; } // Column 11: Mob No1(Father)
        public string? FatherDob { get; set; } // Column 12: DOB(Father)
        public string? FatherBaptismDate { get; set; } // Column 13: Date of Baptsm(Father)
        public string? FatherChrismationDate { get; set; } // Column 14: Date of Chrismation(Father)
        public string? FatherHolyCommunionDate { get; set; } // Column 15: Date of Holy Communion(Father)
        public string? MarriageDate { get; set; } // Column 34: Date of Marriage

        // Mother data
        public string? MotherFirstName { get; set; } // Column 25: First-Name(Mother)
        public string? MotherSurname { get; set; } // Column 26: Surname(Mother)
        public string? MotherBaptismalName { get; set; } // Column 27: Baptismal-Name(Mother)
        public string? MotherContact { get; set; } // Column 33: Mob No2
        public string? MotherDob { get; set; } // Column 30: DOB(Mother)
        public string? MotherBaptismDate { get; set; } // Column 31: Date Of Baptism(Mother)
        public string? MotherChrismationDate { get; set; } // Column 32: Date of Chrismation(Mother)
        public string? MotherHolyCommunionDate { get; set; } // Column 28: Date of Holy Communion(Mother)

        public bool HasFather => !string.IsNullOrEmpty(FatherFirstName);
        public bool HasMother => !string.IsNullOrEmpty(MotherFirstName);
        public bool HasChildren => Children.Any();
        // Children data (up to 6 children)
        public List<ExcelChildData> Children { get; set; } = new List<ExcelChildData>();
    }

    public class ExcelChildData
    {
        public string? FirstName { get; set; }
        public string? Surname { get; set; }
        public string? BaptismalName { get; set; }
        public string? Dob { get; set; }
        public string? BaptismDate { get; set; }
        public string? ChrismationDate { get; set; }
        public string? HolyCommunionDate { get; set; }
    }
}
