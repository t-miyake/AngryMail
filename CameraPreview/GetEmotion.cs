using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CameraPreview
{
    public class GetEmotion
    {
        private readonly FaceAttributeType[] _faceAttributes = { FaceAttributeType.Age, FaceAttributeType.Gender, FaceAttributeType.Emotion };

        public async Task<List<Attributes>> FromFilePathAsync(string filePath, string subscriptionKey, string endpoint)
        {
            var faceClient = new FaceClient(new ApiKeyServiceClientCredentials(subscriptionKey)) { Endpoint = endpoint };
            try
            {
                return  await DetectLocalAsync(faceClient, filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private async Task<List<Attributes>> DetectLocalAsync(IFaceClient faceClient, string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                return null;
            }

            try
            {
                using (Stream imageStream = File.OpenRead(imagePath))
                {
                    var faceList = await faceClient.Face.DetectWithStreamAsync(imageStream, true, false, _faceAttributes);
                    return GetFaceAttributes(faceList);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private List<Attributes> GetFaceAttributes(IEnumerable<DetectedFace> faceList)
        {
            var detectedFaces = faceList.ToList();

            if (!detectedFaces.Any())
            {
                return null;
            }

            return detectedFaces.Select(face => new Attributes
            {
                Age = face.FaceAttributes.Age,
                Gender = face.FaceAttributes.Gender.ToString(),
                Anger = face.FaceAttributes.Emotion.Anger,
                Contempt = face.FaceAttributes.Emotion.Contempt,
                Disgust = face.FaceAttributes.Emotion.Disgust,
                Fear = face.FaceAttributes.Emotion.Fear,
                Happiness = face.FaceAttributes.Emotion.Happiness,
                Neutral = face.FaceAttributes.Emotion.Neutral,
                Sadness = face.FaceAttributes.Emotion.Sadness,
                Surprise = face.FaceAttributes.Emotion.Surprise
            }
            ).ToList();
        }
    }
}
