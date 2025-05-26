using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class GetUserChatsUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;

        public GetUserChatsUseCase(IUserRepository userRepository, IChatRepository chatRepository)
        {
            _userRepository = userRepository;
            _chatRepository = chatRepository;
        }

        public async Task<GetUserChatsResponse> Execute(string userId)
        {
            try
            {
                var userChats = await _chatRepository.GetUserChats(userId);

                return new GetUserChatsResponse
                {
                    IsSuccess = true,
                    Chats = userChats
                };
            }
            catch (Exception ex)
            {
                return new GetUserChatsResponse
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
