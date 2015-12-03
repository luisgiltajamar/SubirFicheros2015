using System;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace SubirFicheros.Utils
{
    public class AzureStorageUtils
    {
        private CloudStorageAccount _cuenta;
        private String _contenedor;

        public AzureStorageUtils(String cuenta, String clave, String contenedor)
        {
            StorageCredentials cred=new StorageCredentials(cuenta,clave);
            _cuenta=new CloudStorageAccount(cred,true);
            _contenedor = contenedor;
        }

        private void ComprobarContenedor(String contenedor=null)
        {
            if (contenedor != null)
            {
                _contenedor = contenedor;
            }

            var cliente=_cuenta.CreateCloudBlobClient();
            var cont = cliente.GetContainerReference(_contenedor);
            cont.CreateIfNotExists();

        }

        public void SubirFichero(Stream fichero, String nombre, String contenedor = null)
        {
            ComprobarContenedor(contenedor);

            var client = _cuenta.CreateCloudBlobClient();
            var cont = client.GetContainerReference(_contenedor);
            var blob = cont.GetBlockBlobReference(nombre);
            blob.UploadFromStream(fichero);
            fichero.Close();
        }

        public byte[] RecuperarFichero(String nombre, String contenedor)
        {
            ComprobarContenedor(contenedor);
            var client = _cuenta.CreateCloudBlobClient();
            var cont = client.GetContainerReference(_contenedor);
            var blob = cont.GetBlockBlobReference(nombre);
            
            blob.FetchAttributes();
            var lon = blob.Properties.Length;
            var dest=new byte[lon];
            blob.DownloadToByteArray(dest, 0);
            return dest;
        }




    }
}