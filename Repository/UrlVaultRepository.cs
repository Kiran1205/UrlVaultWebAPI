using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UrlVaultWebAPI.Model;

namespace UrlVaultWebAPI.Repository
{
    public class UrlVaultRepository
    {
        private List<UrlVaultModel> _cache = new List<UrlVaultModel>();

        private readonly IElasticClient _elasticClient;
        private readonly ILogger _logger;

        public UrlVaultRepository(IElasticClient elasticClient, ILogger<UrlVaultRepository> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
        }
        public  IEnumerable<UrlVaultModel> Get(int UserId)
        {
            var documents =  _elasticClient.Search<UrlVaultModel>(s => s.Query(p => p.Term(x => x.UserId, UserId))).Documents;
            var newdocs = _cache.Except(documents);
            if (newdocs.Any())
            {
                _cache.AddRange(newdocs);
            }            
            return documents;            
        }
        public virtual Task<IEnumerable<UrlVaultModel>> GetUrl( int UserId, int count, int skip = 0)
        {
            var urlVaults = _cache
                .Where(p => p.UserId == UserId)
                .Skip(skip)
                .Take(count);

            return Task.FromResult(urlVaults);
        }
        public IEnumerable<UrlVaultModel> SearchByWordForUser(string searchPhase,int userId)
        {
            var searchResult = _elasticClient.Search<UrlVaultModel>(s => s
                                           .Query(q => q
                                           .Term(x => x.UserId, userId))
                                           .Query(k => k
                                           .MultiMatch(m => m
                                               .Fields(f => f
                                                   .Field("description")
                                                   .Field("name")

                                               )
                                               .Query(searchPhase).Fuzziness(Fuzziness.EditDistance(3))
                                            )
                                        )
                                     ).Documents;

            return searchResult;
        }
        public IEnumerable<UrlVaultModel> SearchByWord(string searchPhase)
        {
            var searchResult =  _elasticClient.Search<UrlVaultModel>(s => s
                                            .Query(q => q
                                            .MultiMatch(m => m
                                                .Fields(f => f
                                                    .Field("description")
                                                    .Field("name")

                                                )
                                                .Query(searchPhase).Fuzziness(Fuzziness.EditDistance(3))
                                             )
                                         )
                                     ).Documents;

            return searchResult;
        }

        public async Task SaveSingleAsync(UrlVaultModel urlVaultModel)
        {
            if (_cache.Any(p => p.Id == urlVaultModel.Id))
            {
                await _elasticClient.UpdateAsync<UrlVaultModel>(urlVaultModel, u => u.Doc(urlVaultModel));
            }
            else
            {
                _cache.Add(urlVaultModel);
                await _elasticClient.IndexDocumentAsync<UrlVaultModel>(urlVaultModel);
            }
        }

        public async Task SaveManyAsync(UrlVaultModel[] urlVaultModels)
        {
            _cache.AddRange(urlVaultModels);
            var result = await _elasticClient.IndexManyAsync(urlVaultModels);
            if (result.Errors)
            {
                // the response can be inspected for errors
                foreach (var itemWithError in result.ItemsWithErrors)
                {
                    _logger.LogError("Failed to index document {0}: {1}",
                        itemWithError.Id, itemWithError.Error);
                }
            }
        }

        public async Task DeleteAsync(UrlVaultModel urlVaultModel)
        {
            await _elasticClient.DeleteAsync<UrlVaultModel>(urlVaultModel);

            if (_cache.Contains(urlVaultModel))
            {
                _cache.Remove(urlVaultModel);
            }
        }

    }
}
