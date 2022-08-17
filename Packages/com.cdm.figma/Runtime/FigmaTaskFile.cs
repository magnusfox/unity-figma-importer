using System.Collections.Generic;
using UnityEngine;

namespace Cdm.Figma
{
    [CreateAssetMenu(fileName = nameof(FigmaTaskFile), menuName = AssetMenuRoot + "Figma Task File", order = 0)]
    public class FigmaTaskFile : ScriptableObject
    {
        protected const string AssetMenuRoot = "Cdm/Figma/";

        [SerializeField]
        private string _personalAccessToken;

        /// <summary>
        /// A personal access token gives the holder access to a Figma account through the API to be able
        /// to download Figma files.
        /// </summary>
        public string personalAccessToken
        {
            get => _personalAccessToken;
            set => _personalAccessToken = value;
        }

        [SerializeField]
        private string _assetExtension = "figma";

        /// <summary>
        /// The extension of the downloaded Figma files. Appropriate Figma asset importer used regarding to the
        /// extension.
        /// </summary>
        public string assetExtension => _assetExtension;

        [SerializeField]
        private string _assetsPath = "Resources/Figma/Files";

        /// <summary>
        /// The directory where downloaded Figma files are stored in.
        /// </summary>
        public string assetsPath => _assetsPath;

        [SerializeField]
        private string _scriptsPath = "Resources/Figma/Scripts";

        public string scriptsPath => _scriptsPath;

        [SerializeField]
        private bool _generateScripts = true;

        public bool generateScripts
        {
            get => _generateScripts;
            set => _generateScripts = value;
        }

        [SerializeField]
        private List<string> _files = new List<string>();

        /// <summary>
        /// Figma file IDs to be downloaded.
        /// </summary>
        public List<string> files => _files;

        public virtual IFigmaDownloader GetDownloader()
        {
            return new FigmaDownloader();
        }
    }
}