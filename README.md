Frenetic Media Server
---------------------

A simple public media server!

### Project Status

Primitive but functional. All the necessities work, but there's no clean home page / registration system / etc. (User registration must be done by editing config files for now).

Code quality is a bit messy, as the project was written once as a vertical slice in 2018 and never really cleaned up since.

### Setup Notes

- This project requires .NET 5 for both editing and running. Be sure to install that first.
- For production running, use a separate webserver (such as Apache) as the access point for this (internal proxy), pointing a public path to `http://localhost:8243/` as a proxy.
    - Also, block external access to port `8243` with your firewall.
- The raw file web URL is specified in the config by `raw_web_url`, and should usually be something like `i.(Site)/i/` which will then automatically have `(Category)/(File).(Ext)` append to the end.
- This seperate file path should be handled by your main webserver, giving direct access to the files in the directory specified in the config by `raw_file_path`.
- Do not grant public access to the directory specified in the config by `meta_file_path`.
- The config to be used should formatted the same as included file `sample_config.cfg`, but placed at local path `config/main.cfg`.
- Must fill any users into the `config/users/` directory based on `sample_user.cfg`. Full path placement might be, for Uploader ID 'bob', `config/users/bob.cfg`
- All config files, directory paths, data files, and meta files have lowercase file names only.

### Public Page Setup

- `(Site)/i/(Category)/(File).(Ext)`: clean HTML-wrapped view of the media file.
- `(Site)/d/(Category)/(File).(Ext)?delete_code=(Code)`: deletes an uploaded file using the deletion code in the meta file.

### POST API

- `(site)/upload`: upload a file.
    - Required: An attached file, with filename attached in the disposition data (file name is needed to see the extension, for media type choice handling).
    - Required: Uploader ID (possibly a username) `uploader_id`.
    - Required: Uploader verification code (possibly a generated password) `uploader_verification`.
    - Required: Category name `file_category` (often the category will be the same as the uploader ID).
    - Optional: Short description text `description` about the file.
    - Response when successful: `success=(Category)/(File).(Ext);(Delete Code)`.
    - Response when failed: `fail=(Reason)`, with reasons...
        - `data`: the POST request has missing or invalid data.
        - `user_verification`: user verification failed (bad uploader ID or verification code).
        - `file_size`: the file is too big to be uploaded.
        - `file_type`: uploader is not allowed to upload that file type (or file type is non-existent / invalid / etc).
        - `category`: that category is not valid for the uploader.
        - `internal`: some internal issue occurred (details are not exposed to user - check server logs).

### ShareX Config File

- I personally upload via [ShareX](https://getsharex.com/), which is quick, easy, and convenient for the task. To configure ShareX to use FreneticMediaServer, create a file labeled `<YOUR_SERVER>.media.sxcu` with the following content:

```json
{
  "Name": "<WEB URL HERE>",
  "DestinationType": "ImageUploader, TextUploader, FileUploader",
  "RequestURL": "https://<WEB URL HERE>/upload",
  "FileFormName": "file_up",
  "Arguments": {
    "file_category": "misc",
    "uploader_verification": "<PUT YOUR PASSCODE HERE>",
    "uploader_id": "<USERNAME HERE>",
    "description": "Uploaded by ShareX.",
    "file_name": "$filename$"
  },
  "RegexList": [
    "^success=([^;]+);(.+)$"
  ],
  "URL": "https://i.<WEB URL HERE>/i/$regex:1,1$",
  "DeletionURL": "https://<WEB URL HERE>/d/$regex:1,1$?delete_code=$regex:1,2$"
}
```

Fill in the `<WEB URL HERE>`, `<USERNAME HERE>`, and `<PUT YOUR PASSCODE HERE>` slots (be sure to remove the `<>` symbols). Replace the `misc` file category with whatever category you prefer. Optionally set the description to something custom. Don't fill in or edit any other parts.

### Main Config Options
- `global_max_file_size`: set to the max file size limit to apply globally.
    - Specify as an integer number followed by B, KB, MB, or GB (which mean: Bytes, KiloBytes, MegaBytes, or GigaBytes).
- `raw_file_path`: set to the local file path to store raw files into.
- `meta_file_path`: set to the local file path to store meta files into.
- `raw_web_url`: set to the web URL to view the raw file.
- `support_email`: set to an email address for support/contact (to display on pages).
- `rebuild_images`: set to `true` or `false` for whether to rebuild any uploaded image as a new `.png` image file (if `false` images will be unmodified).

### User File Options
- `full_name`: the name to display on uploaded file pages.
- `valid_categories_regex`: a regex (regular expression) matcher for a valid category name (must fully match the given category).
- `valid_types_list`: a list of valid upload file types, separated by commas. Specify `*` to allow all. Available specific types: image,animation,video,audio,text.
- `max_file_size`: max upload file size for the user.
- `max_delete_time`: a time duration in which a user may still delete files they have uploaded.
    - Specify as an integer number followed by S, M, H, or D (Which mean: Seconds, Minutes, Hours, or Days).
- `verification_code`: a valid verification code.
    - You can generate a valid code to put here by opening the page at `(Site)/generate_code?pass=(Your Password)`.
    - In that URL, change 'your password' to any password or randomly generated text that you will use as an upload verification code.
    - The page, when loaded, will output a valid hashed verification code that you can copy/paste into the config.
    - The generated code is a single (though slightly long) line, starts with letter 'v', and contains exactly two colon (`:`) symbols.

# Stored File Metadata
- `original_name`: the original file name.
- `time`: the date/time the file was uploaded as a Unix timestamp (seconds since 1970).
- `uploader`: the ID of the uploader who uploaded this file.
- `delete_code`: a generated deletion code for this file.
- `description`: short description text (if the uploader has specified any).

### Running in Development Mode

- **Windows**
    - Launch `WindowsDevStart.bat`.
    - Connect in your browser to the relevant IP (often `localhost`) on port `8243`. (So, page `http://localhost:8243/`).

### Running in Production Mode

- **Linux**
    - If the project was updated (or freshly installed) since the last run (or this is the first run), execute `dotnet restore`.
    - Launch `start.sh` (be sure to add the execute permission bit! Also ensure the line endings have correctly loaded in as LF, not CRLF!).
    - Connect to the path you set up in your primary web server.

### Previous License

Copyright (C) 2018-2021 Frenetic LLC, All Rights Reserved.

### Licensing pre-note:

This is an open source project, provided entirely freely, for everyone to use and contribute to.

If you make any changes that could benefit the community as a whole, please contribute upstream.

### The short of the license is:

You can do basically whatever you want, except you may not hold any developer liable for what you do with the software.

### The long version of the license follows:

The MIT License (MIT)

Copyright (c) 2021 Frenetic LLC

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
