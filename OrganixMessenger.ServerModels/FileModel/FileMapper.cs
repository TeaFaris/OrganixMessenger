namespace OrganixMessenger.ServerModels.FileModel
{
    public static class FileMapper
    {
        public static FileDTO ToDTO(this UploadedFile serverModel)
        {
            return new()
            {
                Id = serverModel.Id,
                FileName = serverModel.FileName,
                ContentType = serverModel.ContentType
            };
        }
    }
}
