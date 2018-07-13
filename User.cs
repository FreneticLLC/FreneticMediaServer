using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FreneticMediaServer
{
    public class User
    {
        public User(string _name, string fileData)
        {
            Name = _name;
            bool typesSet = false, deleteTimeSet = false, fileSizeSet = false;
            foreach (KeyValuePair<string, string> option in GeneralHelpers.ReadConfigData(fileData, (line) => throw new Exception("Invalid user configuration line '" + line + "'")))
            {
                string value = option.Value;
                switch (option.Key)
                {
                    case "full_name":
                        FullName = value;
                        break;
                    case "valid_categories_regex":
                        CategoryVerifier = new Regex("^(" + value + ")$", RegexOptions.Compiled);
                        break;
                    case "valid_types_list":
                        if (value == "*")
                        {
                            CanUploadAnyType = true;
                        }
                        else
                        {
                            AllowedTypes = new HashSet<string>(value.ToLowerInvariant().Split(','));
                        }
                        typesSet = true;
                        break;
                    case "max_file_size":
                        long? maxSizeTemp = GeneralHelpers.ParseFileSizeLimit(value);
                        if (!maxSizeTemp.HasValue)
                        {
                            throw new Exception("Invalid max file size specification!");
                        }
                        MaxFileSize = maxSizeTemp.Value;
                        fileSizeSet = true;
                        break;
                    case "max_delete_time":
                        TimeSpan? timeSpanTemp = GeneralHelpers.ParseTimeSpan(value);
                        if (!timeSpanTemp.HasValue)
                        {
                            throw new Exception("Invalid max delete time specification!");
                        }
                        MaxDeleteTime = timeSpanTemp.Value;
                        deleteTimeSet = true;
                        break;
                    case "verification_code":
                        VerificationCode = value;
                        break;
                    default:
                        continue;
                }
            }
            if (FullName == null)
            {
                throw new Exception("User data does not contain a full name value!");
            }
            if (VerificationCode == null)
            {
                throw new Exception("User data does not contain a verification code value!");
            }
            if (CategoryVerifier == null)
            {
                throw new Exception("User data does not contain a category verifier value!");
            }
            if (!typesSet)
            {
                throw new Exception("User data does not contain an allowed types value!");
            }
            if (!deleteTimeSet)
            {
                throw new Exception("User data does not contain a max delete time value!");
            }
            if (!fileSizeSet)
            {
                throw new Exception("User data does not contain a max file size value!");
            }
        }

        public string Name;

        public string FullName;

        public long MaxFileSize;

        public bool CanUploadAnyType = false;

        public HashSet<string> AllowedTypes = new HashSet<string>();

        public TimeSpan MaxDeleteTime;

        public string VerificationCode;

        public Regex CategoryVerifier;

        public bool CanUploadType(MediaType type)
        {
            return CanUploadAnyType || AllowedTypes.Contains(type.Name);
        }
    }
}
