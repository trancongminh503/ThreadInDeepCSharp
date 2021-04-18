using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RecordDemo
{
    public class StudentAttribute : Attribute
    {
        private PropertyInfo _schoolKey;
        public string KeyName { get; set; }
        public string X { get; set; }

        public StudentAttribute(Type callerType, string keyName, [CallerMemberName] string x = "")
        {
            KeyName = keyName;
            X = x;
        }
    }

    public class Student
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }

        [Student(typeof(School), "Name")]
        public School StudentSchool { get; set; }
    }

    public class School
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
