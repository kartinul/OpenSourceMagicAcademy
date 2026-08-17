using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class MakeWebGLFullscreen
{
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.WebGL)
        {
            string indexPath = Path.Combine(pathToBuiltProject, "index.html");
            if (File.Exists(indexPath))
            {
                string html = File.ReadAllText(indexPath);

                // Update the JavaScript to use 100% width and height
                html = Regex.Replace(html, @"canvas\.style\.width = "".*?"";", "canvas.style.width = \"100%\";");
                html = Regex.Replace(html, @"canvas\.style\.height = "".*?"";", "canvas.style.height = \"100%\";");

                // Inject CSS styles to remove padding/margins and overlay the fullscreen button
                string cssInjection = @"
    <style>
      body, html { margin: 0; padding: 0; width: 100%; height: 100%; overflow: hidden; background-color: #000; }
      #unity-container { width: 100% !important; height: 100% !important; }
      #unity-canvas { width: 100% !important; height: 100% !important; }
      #unity-footer { display: block !important; position: absolute; bottom: 0; right: 0; background: transparent !important; box-shadow: none !important; }
      #unity-webgl-logo { display: none !important; }
      #unity-build-title { display: none !important; }
      #unity-fullscreen-button { position: fixed; bottom: 15px; right: 15px; z-index: 9999; filter: invert(1); opacity: 0.5; transition: opacity 0.2s; }
      #unity-fullscreen-button:hover { opacity: 1.0; }
    </style>
  </head>";
                html = html.Replace("</head>", cssInjection);

                // Add click-to-start overlay logic
                string originalInit = "document.querySelector(\"#unity-loading-bar\").style.display = \"none\";";
                string newInit = originalInit + @"
                var overlay = document.createElement('div');
                overlay.id = 'start-overlay';
                overlay.style.cssText = 'position: absolute; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.9); color: white; display: flex; justify-content: center; align-items: center; font-size: 28px; font-family: sans-serif; cursor: pointer; z-index: 10000; flex-direction: column;';
                overlay.innerHTML = '<div>Click here to go fullscreen</div><div style=""font-size: 16px; margin-top: 10px; opacity: 0.7;"">and start the game</div>';
                document.body.appendChild(overlay);
                overlay.onclick = () => {
                  document.body.requestFullscreen();
                  overlay.style.display = 'none';
                };
";
                html = html.Replace(originalInit, newInit);

                File.WriteAllText(indexPath, html);
                Debug.Log("WebGL index.html automatically modified to be fullscreen by default.");
            }
        }
    }
}
