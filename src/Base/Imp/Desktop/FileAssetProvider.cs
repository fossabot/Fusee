﻿using Fusee.Base.Common;
using Fusee.Base.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FileMode = System.IO.FileMode;

namespace Fusee.Base.Imp.Desktop
{
    /// <summary>
    /// Asset provider for direct file access. Typically used in desktop builds where assets are simply contained within
    /// a subdirectory of the application.
    /// </summary>
    public class FileAssetProvider : StreamAssetProvider
    {
        private List<string> _baseDirs;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAssetProvider"/> class.
        /// </summary>
        /// <param name="baseDir">The base directory where assets should be looked for.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public FileAssetProvider(string baseDir = null) : base()
        {
            Init(baseDir == null ? null : new[] { baseDir });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAssetProvider"/> class.
        /// </summary>
        /// <param name="baseDirs">A list of base directories where assets should be looked for.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public FileAssetProvider(IEnumerable<string> baseDirs = null) : base()
        {
            Init(baseDirs);
        }

        private void Init(IEnumerable<string> baseDirs)
        {
            _baseDirs = new List<string>();
            if (baseDirs == null)
            {
                _baseDirs.Add(AppDomain.CurrentDomain.BaseDirectory);
            }
            else
            {
                foreach (var baseDir in baseDirs)
                {
                    if (!Directory.Exists(baseDir))
                        throw new ArgumentException($"Asset base directory \"{baseDir}\"does not exist.", nameof(baseDir));
                    _baseDirs.Add(baseDir);
                }
            }
            // Image handler
            RegisterTypeHandler(new AssetHandler
            {
                ReturnedType = typeof(ImageData),
                Decoder = (string id, object storage) =>
                {
                    var ext = Path.GetExtension(id).ToLower();
                    return ext switch
                    {
                        ".jpg" or ".jpeg" or ".png" or ".bmp" => FileDecoder.LoadImage((Stream)storage),
                        _ => null,
                    };
                },
                DecoderAsync = async (string id, object storage) =>
                {
                    var ext = Path.GetExtension(id).ToLower();
                    return ext switch
                    {
                        ".jpg" or ".jpeg" or ".png" or ".bmp" => await FileDecoder.LoadImageAsync((Stream)storage).ConfigureAwait(false),
                        _ => null,
                    };
                },
                Checker = (string id) =>
                {
                    var ext = Path.GetExtension(id).ToLower();
                    return ext switch
                    {
                        ".jpg" or ".jpeg" or ".png" or ".bmp" => true,
                        _ => false,
                    };
                }
            });

            // Text file -> String handler. Keep this one the last entry as it doesn't check the extension
            RegisterTypeHandler(new AssetHandler
            {
                ReturnedType = typeof(string),
                Decoder = (string _, object storage) =>
                {
                    string ret;
                    using (var sr = new StreamReader((Stream)storage, System.Text.Encoding.Default, true))
                    {
                        ret = sr.ReadToEnd();
                    }
                    return ret;
                },
                DecoderAsync = async (string _, object storage) =>
                {
                    using var sr = new StreamReader((Stream)storage, System.Text.Encoding.Default, true);
                    return await sr.ReadToEndAsync().ConfigureAwait(false);
                },
                Checker = _ => true // If it's there, we can handle it...
            });
        }

        /// <summary>
        /// Creates a stream for the asset identified by id using <see cref="FileStream"/>
        /// </summary>
        /// <param name="id">The asset identifier.</param>
        /// <returns>
        /// A valid stream for reading if the asset ca be retrieved. null otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        protected override Stream GetStream(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            // If it is an absolute path (e.g. C:\SomeDir\AnAssetFile.ext) open it directly
            if (Path.IsPathRooted(id))
                return new FileStream(id, FileMode.Open);

            // Path seems relative. First see if the file exists at the current working directory
            if (File.Exists(id))
                return new FileStream(id, FileMode.Open);

            // At last, look at the specified base directories
            foreach (var baseDir in _baseDirs)
            {
                string path = Path.Combine(baseDir, id);
                if (File.Exists(path))
                    return new FileStream(path, FileMode.Open);
            }
            return null;
        }

        /// <summary>
        /// Checks the existence of the identified asset using <see cref="File.Exists"/>
        /// </summary>
        /// <param name="id">The asset identifier.</param>
        /// <returns>
        /// true if a stream can be created.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        protected override bool CheckExists(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            // If it is an absolute path (e.g. C:\SomeDir\AnAssetFile.ext) directly check its presence
            if (Path.IsPathRooted(id))
                return File.Exists(id);

            // Path seems relative. First see if the file exists at the current working directory
            if (File.Exists(id))
                return true;

            foreach (var baseDir in _baseDirs)
            {
                string path = Path.Combine(baseDir, id);
                if (File.Exists(path))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Create an async stream for the asset identified by id.
        /// </summary>
        /// <param name="id">The asset identifier.</param>
        /// <returns>Implementors should return null if the asset cannot be retrieved. Otherwise returns a file stream to the asset.</returns>
        protected override Stream GetStreamAsync(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            // If it is an absolute path (e.g. C:\SomeDir\AnAssetFile.ext) open it directly
            if (Path.IsPathRooted(id))
                return new FileStream(id, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true); // open stream async

            // Path seems relative. First see if the file exists at the current working directory
            if (File.Exists(id))
                return new FileStream(id, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true); // open stream async

            // At last, look at the specified base directories
            foreach (var baseDir in _baseDirs)
            {
                string path = Path.Combine(baseDir, id);
                if (File.Exists(path))
                    return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true); // open stream async
            }
            return null;

        }

        /// <summary>
        /// Checks the existence of the identified asset as an async method.
        /// </summary>
        /// <param name="id">The asset identifier.</param>
        /// <returns>Implementors should return true if a stream can be created.</returns>
        protected override Task<bool> CheckExistsAsync(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            // If it is an absolute path (e.g. C:\SomeDir\AnAssetFile.ext) directly check its presence
            if (Path.IsPathRooted(id))
                return Task.FromResult(File.Exists(id));

            // Path seems relative. First see if the file exists at the current working directory
            if (File.Exists(id))
                return Task.FromResult(true);

            foreach (var baseDir in _baseDirs)
            {
                string path = Path.Combine(baseDir, id);
                if (File.Exists(path))
                    return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}