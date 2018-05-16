/** 
 * Copyright (C) 2015 langboost
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace org.whispersystems.curve25519.csharp
{

    public class Fe_isnonzero
    {

        //CONVERT #include "fe.h"
        //CONVERT #include "crypto_verify_32.crypto_verify_32.h"

        /*
        return 1 if f == 0
        return 0 if f != 0

        Preconditions:
           |f| bounded by 1.1*2^26,1.1*2^25,1.1*2^26,1.1*2^25,etc.
        */

        static readonly byte[] zero = new byte[32];

        public static int fe_isnonzero(int[] f)
        {
            byte[] s = new byte[32];
            Fe_tobytes.fe_tobytes(s, f);
            return Crypto_verify_32.crypto_verify_32(s, zero);
        }


    }
}
