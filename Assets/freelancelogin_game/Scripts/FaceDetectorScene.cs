namespace OpenCvSharp.Demo
{
	using UnityEngine;
	using UnityEngine.UI;
	using OpenCvSharp;
	using UnityEditor;

	public class FaceDetectorScene : WebCamera
	{
		bool scoreAdded;

        int score;
        [SerializeField] Text scoreText;

        TextAsset faces;

        [SerializeField] RectTransform rectTransform;

        [Space(10)]
        [SerializeField] Transform line;

        private FaceProcessorLive<WebCamTexture> processor;

		/// <summary>
		/// Default initializer for MonoBehavior sub-classes
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
		
			forceFrontalCamera = true; // we work with frontal cams here, let's force it for macOS s MacBook doesn't state frontal cam correctly

			faces = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/freelancelogin_game/Haarcascades/haarcascade_frontalface_default.xml", typeof(TextAsset));
			processor = new FaceProcessorLive<WebCamTexture>();
			processor.Initialize(faces.text);

			// data stabilizer - affects face rects, face landmarks etc.
			processor.DataStabilizer.Enabled = true;        // enable stabilizer
			processor.DataStabilizer.Threshold = 2.0;       // threshold value in pixels
			processor.DataStabilizer.SamplesCount = 2;      // how many samples do we need to compute stable data

			// performance data - some tricks to make it work faster
			processor.Performance.Downscale = 256;          // processed image is pre-scaled down to N px by long side
			processor.Performance.SkipRate = 0;             // we actually process only each Nth frame (and every frame for skipRate = 0)
		}

		/// <summary>
		/// Per-frame video capture processor
		/// </summary>
		protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
		{
			// detect everything we're interested in
			processor.ProcessTexture(input, TextureParameters);

			// mark detected objects
			processor.MarkDetected();

			// processor.Image now holds data we'd like to visualize
			output = Unity.MatToTexture(processor.Image, output);   // if output is valid texture it's buffer will be re-used, otherwise it will be re-created

            if (processor.Faces.Count > 0)
            {
                Vector2 screenPoint = Camera.main.WorldToScreenPoint(line.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, Camera.main, out Vector2 localPosition);

                localPosition.x += rectTransform.sizeDelta.x / 2;
                localPosition.y = rectTransform.sizeDelta.y / 2 - localPosition.y;


                if (processor.Faces[0].Region.Bottom < localPosition.y)
                {
					if(!scoreAdded)
					{
                        score++;
                        scoreText.text = $"score: {score}";
                        scoreAdded = true;
                    }
                }
				else
				{
					scoreAdded = false;
				}
            }
			else
			{
                scoreAdded = false;
            }

            return true;
		}
	}
}