using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using FreneticMediaServer.MediaTypes;
using Microsoft.Extensions.Primitives;
using System.Threading;

namespace FreneticMediaServer
{
    public class Startup
    {
        public static readonly UTF8Encoding EncodingUTF8 = GeneralHelpers.EncodingUTF8;

        public static void LogWarning(string message)
        {
            Console.WriteLine(DateTime.Now.ToString() + " [Warning] " + message);
        }

        public long GlobalMaxFileSize = 0;

        public string RawWebUrl = null;

        public string MetaFilePath = null;

        public string RawFilePath = null;

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public static readonly Object[] FileLockers = new Object[256];

        public static readonly Object[] UserLockers = new Object[256];

        static Startup()
        {
            for (int i = 0; i < FileLockers.Length; i++)
            {
                FileLockers[i] = new Object();
            }
            for (int i = 0; i < UserLockers.Length; i++)
            {
                UserLockers[i] = new Object();
            }
        }

        public static Object PickFileLockFor(string text)
        {
            return FileLockers[text.GetHashCode() & 255];
        }

        public static Object PickUserLockFor(string name)
        {
            return UserLockers[name.GetHashCode() & 255];
        }

        public ConcurrentDictionary<string, User> KnownUsers = new ConcurrentDictionary<string, User>();

        public bool ValidateCleanTextInputLine(string input)
        {
            if (input.Length == 0)
            {
                return false;
            }
            for (int i = 0; i < input.Length; i++)
            {
                if (!(
                    (input[i] >= 'A' && input[i] <= 'Z')
                    || (input[i] >= 'a' && input[i] <= 'z')
                    || (input[i] >= '0' && input[i] <= '9')
                    || (input[i] == '_')
                    ))
                {
                    return false;
                }
            }
            return true;
        }

        public User ReadUserFile(string name)
        {
            lock (PickUserLockFor(name))
            {
                if (KnownUsers.TryGetValue(name, out User usr))
                {
                    return usr;
                }
                string fpath = "./config/users/" + name + ".cfg";
                if (!File.Exists(fpath))
                {
                    return null;
                }
                string file = File.ReadAllText(fpath);
                usr = new User(name, file);
                return usr;
            }
        }

        public User GetUser(string name)
        {
            return KnownUsers.GetOrAdd(name.ToLowerInvariant(), ReadUserFile);
        }

        public void ApplyConfigSetting(string setting, string value)
        {
            switch (setting)
            {
                case "raw_web_url":
                    RawWebUrl = value;
                    break;
                case "meta_file_path":
                    MetaFilePath = value;
                    break;
                case "raw_file_path":
                    RawFilePath = value;
                    break;
                case "global_max_file_size":
                    long? parsedFileSize = GeneralHelpers.ParseFileSizeLimit(value);
                    if (!parsedFileSize.HasValue)
                    {
                        throw new FormatException("Invalid file size limit value '" + value + "' ... must be an integer number followed by B, KB, MB, or GB");
                    }
                    GlobalMaxFileSize = parsedFileSize.Value;
                    break;
                default:
                    LogWarning("Unknown config setting '" + setting + "'");
                    break;
            }
        }

        public void LoadConfig()
        {
            if (!File.Exists("./config/main.cfg"))
            {
                throw new Exception("Config file not available. Please create a file at './config/main.cfg' based on the sample config.");
            }
            string config_data = File.ReadAllText("./config/main.cfg");
            foreach (KeyValuePair<string, string> option in GeneralHelpers.ReadConfigData(config_data, (line) => LogWarning("Invalid configuration line '" + line + "'")))
            {
                ApplyConfigSetting(option.Key, option.Value);
            }
            if (GlobalMaxFileSize <= 0)
            {
                throw new Exception("Config MUST specify a global max file size!");
            }
            if (RawWebUrl == null)
            {
                throw new Exception("Config MUST specify a raw web URL!");
            }
            if (MetaFilePath == null)
            {
                throw new Exception("Config MUST specify a meta file path!");
            }
            if (RawFilePath == null)
            {
                throw new Exception("Config MUST specify a raw file path!");
            }
        }

        public Dictionary<string, MediaType> KnownMediaTypes = new Dictionary<string, MediaType>(128);

        public void RegisterMediaType(MediaType type)
        {
            foreach (string ext in type.GetValidExtensions())
            {
                KnownMediaTypes.Add(ext, type);
            }
        }

        public void EstablishMediaHandlers()
        {
            RegisterMediaType(new ImageMediaType());
            // TODO: animation (gif)
            // TODO: video (mp4, webm, mpeg, avi)
            // TODO: audio (mp3, wav, ogg)
            // TODO: text (txt)
            // TODO: Possibly, code text files? (Pastebin with highlighting)
        }

        public void SetupHttpHeaders(HttpContext context, int code)
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.StatusCode = code;
        }

        public async Task Write(HttpContext context, string text)
        {
            await context.Response.WriteAsync(text, EncodingUTF8);
        }

        public async Task<bool> HandlePage_Get_FileView(HttpContext context, string subPath)
        {
            subPath = subPath.ToLowerInvariant();
            int slash_index = subPath.IndexOf('/');
            if (slash_index < 1)
            {
                return false;
            }
            string category = subPath.Substring(0, slash_index);
            string file_with_ext = subPath.Substring(slash_index + 1);
            if (file_with_ext.Contains('/'))
            {
                return false;
            }
            int ext_index = file_with_ext.IndexOf('.');
            if (ext_index < 1)
            {
                return false;
            }
            string file = file_with_ext.Substring(0, ext_index);
            string ext = file_with_ext.Substring(ext_index + 1);
            if (!KnownMediaTypes.TryGetValue(ext, out MediaType type))
            {
                return false;
            }
            SetupHttpHeaders(context, 200);
            await Write(context, type.GenerateHtmlPageFor(category, file, ext));
            return true;
        }

        public async Task HandlePage_Get_GenerateCode(HttpContext context, string code)
        {
            SetupHttpHeaders(context, 200);
            await Write(context, HtmlHelper.BasicHeaderWithTitle("Generated Code"));
            await Write(context, "Code generated:\n<br>\n<br>" + SecurityHelper.HashCurrent(code) + "\n<br>\n");
            await Write(context, HtmlHelper.BasicFooter());
        }

        public async Task HandlePage_404(HttpContext context)
        {
            SetupHttpHeaders(context, 404);
            await Write(context, HtmlHelper.BasicHeaderWithTitle("404 File Not Found"));
            await Write(context, "<h1>404</h1>\n<br><h2>File Not Found</h2>\n");
            await Write(context, HtmlHelper.BasicFooter());
        }

        public string RandomHexID(int length = 3)
        {
            return GeneralHelpers.BytesToHex(SecurityHelper.GetRandomBytes(length)).ToLowerInvariant();
        }

        public async Task HandlePage_Error(HttpContext context, int code, string error)
        {
            context.Response.ContentType = "text/plain; charset=utf-8";
            context.Response.StatusCode = code;
            await Write(context, "fail=" + error);
        }

        public string SaveUploadFile(MetaFile metaFile, string category, string extension, byte[] data)
        {
            string metaFileString = metaFile.FileOutputString();
            int current_length = 3;
            string fileID = RandomHexID(current_length);
            int attempts = 0;
            string metaPathPrefix = MetaFilePath + category + "/";
            string dotExt = "." + extension;
            while (true)
            {
                string path = metaPathPrefix + fileID + dotExt;
                Object lockObject = PickFileLockFor(fileID);
                lock (lockObject)
                {
                    if (!File.Exists(path))
                    {
                        File.WriteAllText(path, metaFileString);
                        break;
                    }
                }
                attempts++;
                if (attempts > 2)
                {
                    attempts = 0;
                    current_length++;
                    if (current_length > 8)
                    {
                        throw new Exception("File handling error, or code generator broke? Could not generate valid save ID.");
                    }
                }
                fileID = RandomHexID(current_length);
            }
            string rawPath = RawFilePath + category + "/" + fileID + dotExt;
            File.WriteAllBytes(rawPath, data);
            return fileID;
        }

        public async Task HandlePage_Post_Upload(HttpContext context)
        {
            try
            {
                if (context.Request.Form.Files.Count == 0)
                {
                    await HandlePage_Error(context, 400, "data");
                    return;
                }
                IFormFile file = context.Request.Form.Files[0];
                if (file.Length > GlobalMaxFileSize)
                {
                    await HandlePage_Error(context, 400, "file_size");
                    return;
                }
                string filename = file.FileName;
                int indexDot = filename.IndexOf('.');
                if (indexDot < 1)
                {
                    await HandlePage_Error(context, 400, "file_type");
                    return;
                }
                string extension = filename.Substring(indexDot + 1);
                if (!context.Request.Form.TryGetValue("uploader_id", out StringValues uploader_id_val)
                    || !context.Request.Form.TryGetValue("uploader_verification", out StringValues uploader_verification_val)
                    || !context.Request.Form.TryGetValue("file_category", out StringValues file_category_val)
                    || !context.Request.Form.TryGetValue("description", out StringValues description_val))
                {
                    await HandlePage_Error(context, 400, "data");
                    return;
                }
                string uploaderID = uploader_id_val[0].ToLowerInvariant();
                string category = file_category_val[0].ToLowerInvariant();
                string description = description_val[0];
                User user = GetUser(uploaderID);
                if (user == null)
                {
                    await HandlePage_Error(context, 400, "user_verification");
                    return;
                }
                if (!SecurityHelper.CheckHashValidity(user.VerificationCode, uploader_verification_val[0]))
                {
                    await HandlePage_Error(context, 400, "user_verification");
                    return;
                }
                if (!KnownMediaTypes.TryGetValue(extension, out MediaType type))
                {
                    await HandlePage_Error(context, 400, "file_type");
                    return;
                }
                if (!user.CanUploadType(type))
                {
                    await HandlePage_Error(context, 400, "file_type");
                    return;
                }
                if (file.Length > user.MaxFileSize)
                {
                    await HandlePage_Error(context, 400, "file_size");
                    return;
                }
                if (!user.CategoryVerifier.IsMatch(category))
                {
                    await HandlePage_Error(context, 400, "category");
                    return;
                }
                byte[] uploadedData;
                using (MemoryStream file_stream = new MemoryStream((int)file.Length))
                {
                    file.CopyTo(file_stream);
                    uploadedData = file_stream.ToArray();
                }
                MetaFile metaFile = new MetaFile(uploaderID, description, filename);
                string fileID = SaveUploadFile(metaFile, category, extension, uploadedData);
                await Write(context, "success=" + category + "/" + fileID + "." + extension + ";" + metaFile.DeleteCode);
                return;
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                {
                    throw;
                }
                await HandlePage_Error(context, 500, "internal");
                LogWarning("File upload handler: " + ex.ToString());
                return;
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            LoadConfig();
            EstablishMediaHandlers();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.Run(async (context) =>
            {
                if (context.Request.Path.HasValue)
                {
                    if (context.Request.Path.Value.StartsWith("/i/"))
                    {
                        if (await HandlePage_Get_FileView(context, context.Request.Path.Value.Substring("/i/".Length)))
                        {
                            return;
                        }
                    }
                    else if (context.Request.Path.Value.StartsWith("/generate_code"))
                    {
                        if (context.Request.Query.TryGetValue("pass", out StringValues code))
                        {
                            await HandlePage_Get_GenerateCode(context, code[0]);
                            return;
                        }
                    }
                    else if (context.Request.Path.Value.StartsWith("/upload"))
                    {
                        if (context.Request.Method == "POST" && context.Request.HasFormContentType)
                        {
                            await HandlePage_Post_Upload(context);
                        }
                    }
                }
                await HandlePage_404(context);
            });
        }
    }
}
