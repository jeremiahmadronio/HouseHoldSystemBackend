namespace WebApplication2.dto.GamesDTO
{
	public class SubmitAnswerDTO
	{

		public Guid gameId {  get; set; }
		public string? UserChoice { get; set; }
		public int? mock_answer {  get; set; }

	}

}
