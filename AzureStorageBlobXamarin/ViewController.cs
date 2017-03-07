using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using AVFoundation;
using Foundation;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using UIKit;

namespace AzureStorageBlobXamarin
{
	public partial class ViewController : UIViewController
	{
		string archivoLocal;
		AVCaptureDevice dispositivoCaptura;
		AVCaptureSession sesionCaptura;
		AVCaptureDeviceInput entradaDispositivo;
		AVCaptureStillImageOutput salidaImagen;
		AVCaptureVideoPreviewLayer preview;
		string ruta;
		byte[] arregloJPG;

		protected ViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public override async void ViewDidLoad()
		{
			base.ViewDidLoad();
			// Perform any additional setup after loading the view, typically from a nib.

			await autorizacionCamara();
			ConfiguracionCamara();

			btnCapturar.TouchUpInside += async delegate
			{
				var salidaVideo = salidaImagen.ConnectionFromMediaType(AVMediaType.Video);
				var bufferVideo = await salidaImagen.CaptureStillImageTaskAsync(salidaVideo);
				var datosImagen = AVCaptureStillImageOutput.JpegStillToNSData(bufferVideo);

				arregloJPG = datosImagen.ToArray();
				string rutaCarpeta = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				string resultado = "FotoLAB12";
				archivoLocal = resultado + ".jpg";
				ruta = Path.Combine(rutaCarpeta, archivoLocal);
				File.WriteAllBytes(ruta, arregloJPG);
				imageView.Image = UIImage.FromFile(ruta);
			};

			btnRespaldar.TouchUpInside += async delegate
			{
				try
				{
					CloudStorageAccount cuentaAlmacenamiento = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=tallerxamarin;AccountKey=s+A8siTK0j504BTPkIBUT3e05t2OBoddrEXTkBMAbk1gEOH3ry7Vcs0ROAA0CPwfd9xL57Y1ywim+i+nDUNV5w==");
					CloudBlobClient clienteBlob = cuentaAlmacenamiento.CreateCloudBlobClient();
					CloudBlobContainer contenedor = clienteBlob.GetContainerReference("imagenes");
					CloudBlockBlob recursoBlob = contenedor.GetBlockBlobReference(archivoLocal);

					await recursoBlob.UploadFromFileAsync(ruta);
					MessageBox("Guardado en", "Azure Storage -  Blob");
				}
				catch (StorageException ex)
				{
					MessageBox("Error:", ex.Message);
				}


			};
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}

		public static void MessageBox(string Title, string message)
		{
			var Alerta = new UIAlertView();
			Alerta.Title = Title;
			Alerta.Message = message;
			Alerta.AddButton("OK");
			Alerta.Show();
		}

		async Task autorizacionCamara() {
			var estatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);
			if (estatus != AVAuthorizationStatus.Authorized) {
				await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Video);
			}
		}

		public void ConfiguracionCamara() {
			sesionCaptura = new AVCaptureSession();
			preview = new AVCaptureVideoPreviewLayer(sesionCaptura)
			{
				Frame = new RectangleF(40, 50, 300, 300)
			};

			View.Layer.AddSublayer(preview);
			dispositivoCaptura = AVCaptureDevice.GetDefaultDevice(AVMediaType.Video);

			entradaDispositivo = AVCaptureDeviceInput.FromDevice(dispositivoCaptura);
			sesionCaptura.AddInput(entradaDispositivo);
			salidaImagen = new AVCaptureStillImageOutput()
			{
				OutputSettings = new NSDictionary()
			};
			sesionCaptura.AddOutput(salidaImagen);
			sesionCaptura.StartRunning();
		}
	}
}
