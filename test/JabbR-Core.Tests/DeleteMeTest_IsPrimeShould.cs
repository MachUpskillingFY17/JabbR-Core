using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JabbR_Core.Configuration;
using Xunit;

namespace JabbR_Core.Tests
{
    public class DeleteMeTest_IsPrimeShould
    {
        private DeleteMeTest _deleteMeTest;

        public DeleteMeTest_IsPrimeShould()
        {
            _deleteMeTest = new DeleteMeTest();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        public void ReturnFalseLessThanTwo(int value)
        {
            var result = _deleteMeTest.IsPrime(value);

            Assert.False(result, $"{value} must be greater than 1");

            Console.WriteLine("DeleteMeTest_IsPrimeShould.ReturnFalseLessThanTwo: Complete");
        }

    }
}
