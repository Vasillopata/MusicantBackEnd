namespace MusicantBackEnd.Models.InputModels
{
    public class RegisterInputModel
    {
        public string Email {  get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
