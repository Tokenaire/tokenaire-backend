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

using System;

namespace org.whispersystems.curve25519.csharp
{

    public class Curve_sigs
    {

        public static void curve25519_keygen(byte[] curve25519_pubkey_out,
                               byte[] curve25519_privkey_in)
        {
            Ge_p3 ed = new Ge_p3(); /* Ed25519 pubkey point */
            int[] ed_y = new int[10];
            int[] ed_y_plus_one = new int[10];
            int[] one_minus_ed_y = new int[10];
            int[] inv_one_minus_ed_y = new int[10];
            int[] mont_x = new int[10];

            /* Perform a fixed-base multiplication of the Edwards base point,
               (which is efficient due to precalculated tables), then convert
               to the Curve25519 montgomery-format public key.  In particular,
               convert Curve25519's "montgomery" x-coordinate into an Ed25519
               "edwards" y-coordinate:

               mont_x = (ed_y + 1) / (1 - ed_y)

               with projective coordinates:

               mont_x = (ed_y + ed_z) / (ed_z - ed_y)

               NOTE: ed_y=1 is converted to mont_x=0 since fe_invert is mod-exp
            */

            Ge_scalarmult_base.ge_scalarmult_base(ed, curve25519_privkey_in);
            Fe_add.fe_add(ed_y_plus_one, ed.Y, ed.Z);
            Fe_sub.fe_sub(one_minus_ed_y, ed.Z, ed.Y);
            Fe_invert.fe_invert(inv_one_minus_ed_y, one_minus_ed_y);
            Fe_mul.fe_mul(mont_x, ed_y_plus_one, inv_one_minus_ed_y);
            Fe_tobytes.fe_tobytes(curve25519_pubkey_out, mont_x);
        }

        public static int curve25519_sign(ISha512 sha512provider, byte[] signature_out,
                            byte[] curve25519_privkey,
                            byte[] msg, int msg_len,
                            byte[] random)
        {
            Ge_p3 ed_pubkey_point = new Ge_p3(); /* Ed25519 pubkey point */
            byte[] ed_pubkey = new byte[32]; /* Ed25519 encoded pubkey */
            byte[] sigbuf = new byte[msg_len + 128]; /* working buffer */
            byte sign_bit = 0;

            /* Convert the Curve25519 privkey to an Ed25519 public key */
            Ge_scalarmult_base.ge_scalarmult_base(ed_pubkey_point, curve25519_privkey);
            Ge_p3_tobytes.ge_p3_tobytes(ed_pubkey, ed_pubkey_point);
            sign_bit = (byte)(ed_pubkey[31] & 0x80);

            /* Perform an Ed25519 signature with explicit private key */
            sign_modified.crypto_sign_modified(sha512provider, sigbuf, msg, msg_len, curve25519_privkey,
                                               ed_pubkey, random);
            Array.Copy(sigbuf, 0, signature_out, 0, 64);

            /* Encode the sign bit into signature (in unused high bit of S) */
            signature_out[63] &= 0x7F; /* bit should be zero already, but just in case */
            signature_out[63] |= sign_bit;
            return 0;
        }

        public static int curve25519_verify(ISha512 sha512provider, byte[] signature,
                              byte[] curve25519_pubkey,
                              byte[] msg, int msg_len)
        {
            int[] mont_x = new int[10];
            int[] mont_x_minus_one = new int[10];
            int[] mont_x_plus_one = new int[10];
            int[] inv_mont_x_plus_one = new int[10];
            int[] one = new int[10];
            int[] ed_y = new int[10];
            byte[] ed_pubkey = new byte[32];
            long some_retval = 0;
            byte[] verifybuf = new byte[msg_len + 64]; /* working buffer */
            byte[] verifybuf2 = new byte[msg_len + 64]; /* working buffer #2 */

            /* Convert the Curve25519 public key into an Ed25519 public key.  In
               particular, convert Curve25519's "montgomery" x-coordinate into an
               Ed25519 "edwards" y-coordinate:

               ed_y = (mont_x - 1) / (mont_x + 1)

               NOTE: mont_x=-1 is converted to ed_y=0 since fe_invert is mod-exp

               Then move the sign bit into the pubkey from the signature.
            */
            Fe_frombytes.fe_frombytes(mont_x, curve25519_pubkey);
            Fe_1.fe_1(one);
            Fe_sub.fe_sub(mont_x_minus_one, mont_x, one);
            Fe_add.fe_add(mont_x_plus_one, mont_x, one);
            Fe_invert.fe_invert(inv_mont_x_plus_one, mont_x_plus_one);
            Fe_mul.fe_mul(ed_y, mont_x_minus_one, inv_mont_x_plus_one);
            Fe_tobytes.fe_tobytes(ed_pubkey, ed_y);

            /* Copy the sign bit, and remove it from signature */
            ed_pubkey[31] &= 0x7F;  /* bit should be zero already, but just in case */
            ed_pubkey[31] |= (byte)(signature[63] & 0x80);
            Array.Copy(signature, 0, verifybuf, 0, 64);
            verifybuf[63] &= 0x7F;

            Array.Copy(msg, 0, verifybuf, 64, (int)msg_len);

            /* Then perform a normal Ed25519 verification, return 0 on success */
            /* The below call has a strange API: */
            /* verifybuf = R || S || message */
            /* verifybuf2 = java to next call gets a copy of verifybuf, S gets
               replaced with pubkey for hashing, then the whole thing gets zeroized
               (if bad sig), or contains a copy of msg (good sig) */
            return Open.crypto_sign_open(sha512provider, verifybuf2, some_retval, verifybuf, 64 + msg_len, ed_pubkey);
        }
    }
}
