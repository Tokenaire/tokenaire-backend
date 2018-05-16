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

    public class Arrays
    {
        /**
         * Assigns the specified byte value to each element of the specified array
         * of bytes.
         *
         * @param a the array to be filled
         * @param val the value to be stored in all elements of the array
         */
        public static void fill(byte[] a, byte val)
        {
            for (int i = 0, len = a.Length; i < len; i++)
                a[i] = val;
        }

    }
}
