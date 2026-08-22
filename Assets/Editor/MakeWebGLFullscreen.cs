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

        // Inject CSS styles to remove padding/margins and hide footer/fullscreen buttons
        string cssInjection = @"
    <style>
      body, html { margin: 0; padding: 0; width: 100%; height: 100%; overflow: hidden; background-color: #000; font-family: sans-serif; }
      #unity-container { width: 100% !important; height: 100% !important; position: absolute; top: 0; left: 0; }
      #unity-canvas { width: 100% !important; height: 100% !important; }
      #unity-footer { display: none !important; }
      #unity-webgl-logo { display: none !important; }
      #unity-build-title { display: none !important; }
      #unity-fullscreen-button { display: none !important; }
    </style>
  </head>";
        html = html.Replace("</head>", cssInjection);

        // Add mobile landscape & fullscreen enforcement logic
        string originalInit = "document.querySelector(\"#unity-loading-bar\").style.display = \"none\";";
        string newInit = originalInit + @"
                var isMobile = /iPhone|iPad|iPod|Android|webOS|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) ||
                               (navigator.maxTouchPoints && navigator.maxTouchPoints > 2 && /Macintosh/i.test(navigator.userAgent));

                function requestFullscreen(element) {
                  var el = element || document.documentElement;
                  if (el.requestFullscreen) {
                    el.requestFullscreen().catch(function() {});
                  } else if (el.webkitRequestFullscreen) {
                    el.webkitRequestFullscreen();
                  } else if (el.mozRequestFullScreen) {
                    el.mozRequestFullScreen();
                  } else if (el.msRequestFullscreen) {
                    el.msRequestFullscreen();
                  }
                }

                function isFullscreen() {
                  return !!(document.fullscreenElement || document.webkitFullscreenElement || document.mozFullScreenElement || document.msFullscreenElement);
                }

                function isPortrait() {
                  if (window.matchMedia && window.matchMedia('(orientation: portrait)').matches) {
                    return true;
                  }
                  if (screen.orientation && screen.orientation.type) {
                    return screen.orientation.type.includes('portrait');
                  }
                  return window.innerHeight > window.innerWidth;
                }

                var overlay = document.createElement('div');
                overlay.id = 'start-overlay';
                overlay.style.cssText = 'position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.95); color: white; display: flex; justify-content: center; align-items: center; font-size: 28px; font-family: sans-serif; cursor: pointer; z-index: 10000; flex-direction: column; text-align: center; padding: 20px; box-sizing: border-box;';
                overlay.innerHTML = '<div>Click here to go fullscreen</div><div style=""font-size: 16px; margin-top: 10px; opacity: 0.7;"">and start the game</div>';
                document.body.appendChild(overlay);

                var orientationOverlay = document.createElement('div');
                orientationOverlay.id = 'orientation-overlay';
                orientationOverlay.style.cssText = 'position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: #000; color: white; display: none; justify-content: center; align-items: center; font-size: 24px; font-family: sans-serif; z-index: 20000; flex-direction: column; text-align: center; padding: 20px; box-sizing: border-box;';
                orientationOverlay.innerHTML = '<div>Please rotate your device</div><div style=""font-size: 16px; margin-top: 10px; opacity: 0.7;"">Landscape mode is required to play</div>';
                document.body.appendChild(orientationOverlay);

                overlay.onclick = function() {
                  requestFullscreen(document.documentElement);
                  if (isMobile && screen.orientation && screen.orientation.lock) {
                    screen.orientation.lock('landscape').catch(function() {});
                  }
                  overlay.style.display = 'none';
                };

                function checkState() {
                  if (!isMobile) return;

                  if (isPortrait()) {
                    orientationOverlay.style.display = 'flex';
                  } else {
                    orientationOverlay.style.display = 'none';
                    if (!isFullscreen()) {
                      overlay.style.display = 'flex';
                    }
                  }
                }

                if (isMobile) {
                  ['fullscreenchange', 'webkitfullscreenchange', 'mozfullscreenchange', 'MSFullscreenChange'].forEach(function(evt) {
                    document.addEventListener(evt, checkState);
                  });
                  window.addEventListener('resize', checkState);
                  window.addEventListener('orientationchange', checkState);
                  if (screen.orientation) {
                    screen.orientation.addEventListener('change', checkState);
                  }
                  checkState();
                }
";
        html = html.Replace(originalInit, newInit);

        File.WriteAllText(indexPath, html);
        Debug.Log("WebGL index.html automatically modified for mobile fullscreen & landscape enforcement.");
      }
    }
  }
}
