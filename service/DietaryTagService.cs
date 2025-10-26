using WebApplication2.dto.DietaryTagDTO;
using WebApplication2.repositories;
using WebApplication2.models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace WebApplication2.service
{
    public class DietaryTagService 
    {
        private readonly IDietaryTagRepository _dietaryTagRepository;
        


        public DietaryTagService(IDietaryTagRepository dietaryTagRepository)
        {
            _dietaryTagRepository = dietaryTagRepository;


        }

        //display
        public async Task<IEnumerable<DisplayDietaryTagDTO>> GetAllDietaryTagsAsync()
        {
            var dietaryTags = await _dietaryTagRepository.getAllAsync();

            var dtoList = dietaryTags.Select(dt => new DisplayDietaryTagDTO
            {
                DietaryTagId = dt.DietaryTagId,
                Name = dt.Name,
                Description = dt.Description
            });

            return dtoList;
        }
    }


}

