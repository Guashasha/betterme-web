namespace betterme_web_gui.DTOS
{
    public class VerificationRequestDTO
    {
        public IFormFile? Identification { get; set; }
        public IFormFile? Certificate { get; set; }

        public bool IsValid()
        {
            return Identification != null && Certificate != null;
        }
    }
}