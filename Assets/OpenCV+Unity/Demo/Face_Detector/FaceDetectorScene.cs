namespace OpenCvSharp.Demo
{
	using System;
	using UnityEngine;
	using System.Collections.Generic;
	using UnityEngine.UI;
	using OpenCvSharp;

	public class FaceDetectorScene : WebCamera
	{
		int score;
		[SerializeField] Text scoreText;

		public TextAsset faces;
		public TextAsset eyes;
		public TextAsset shapes;

		public RectTransform rectTransform;

		[Space(10)]
		[SerializeField] Transform line;
		[SerializeField] Text outText;

		private FaceProcessorLive<WebCamTexture> processor;

		/// <summary>
		/// Default initializer for MonoBehavior sub-classes
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			base.forceFrontalCamera = true; // we work with frontal cams here, let's force it for macOS s MacBook doesn't state frontal cam correctly

			byte[] shapeDat = shapes.bytes;
			if (shapeDat.Length == 0)
			{
				string errorMessage =
					"In order to have Face Landmarks working you must download special pre-trained shape predictor " +
					"available for free via DLib library website and replace a placeholder file located at " +
					"\"OpenCV+Unity/Assets/Resources/shape_predictor_68_face_landmarks.bytes\"\n\n" +
					"Without shape predictor demo will only detect face rects.";

#if UNITY_EDITOR
				// query user to download the proper shape predictor
				if (UnityEditor.EditorUtility.DisplayDialog("Shape predictor data missing", errorMessage, "Download", "OK, process with face rects only"))
					Application.OpenURL("http://dlib.net/files/shape_predictor_68_face_landmarks.dat.bz2");
#else
             UnityEngine.Debug.Log(errorMessage);
#endif
			}

			processor = new FaceProcessorLive<WebCamTexture>();
			processor.Initialize(faces.text, eyes.text, shapes.bytes);

			// data stabilizer - affects face rects, face landmarks etc.
			processor.DataStabilizer.Enabled = true;        // enable stabilizer
			processor.DataStabilizer.Threshold = 2.0;       // threshold value in pixels
			processor.DataStabilizer.SamplesCount = 2;      // how many samples do we need to compute stable data

			// performance data - some tricks to make it work faster
			processor.Performance.Downscale = 256;          // processed image is pre-scaled down to N px by long side
			processor.Performance.SkipRate = 0;             // we actually process only each Nth frame (and every frame for skipRate = 0)
		}

		private void Update()
		{
			if(Input.GetMouseButton(0))
			{
				if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
				{
					line.position = new Vector2(line.position.x, Input.mousePosition.y);
				}
			}

			if(processor.Faces.Count > 0)
			{
                Vector2 screenPoint = Camera.main.WorldToScreenPoint(line.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, Camera.main, out Vector2 localPosition);

                localPosition.x += rectTransform.sizeDelta.x / 2;
                localPosition.y = rectTransform.sizeDelta.y / 2 - localPosition.y;

                outText.text = $"Head Y:{processor.Faces[0].Region.Center.Y} & line Y: {localPosition.y}";
                if (processor.Faces[0].Region.Center.Y > localPosition.y)
                {
                    score++;
                    scoreText.text = $"score: {score}";
                }
            }
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

			return true;
		}
	}
}