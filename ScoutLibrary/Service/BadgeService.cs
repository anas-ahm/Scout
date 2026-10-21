using ScoutLibrary.Models;
using ScoutLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScoutLibrary.Service
{
    public class BadgeService
    {
        private readonly ICRUD<Badge> _badgeRepository;
        public BadgeService(ICRUD<Badge> badgeRepository)
        {
            _badgeRepository = badgeRepository;
        }


        public Badge Create(Badge item)
        {
            return _badgeRepository.Create(item);
        }

        public Badge Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<Badge> GetAll()
        {
            return _badgeRepository.GetAll();
        }
    }
}
