using NBitcoin;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;
using NBitcoin.RPC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NBitcoin.Altcoins
{
	// Reference: https://github.com/dogecoin/dogecoin/blob/10a5e93a055ab5f239c5447a5fe05283af09e293/src/chainparams.cpp
	public class BuyCoinPos : NetworkSetBase
	{
		public static BuyCoinPos Instance { get; } = new BuyCoinPos();

		public override string CryptoCode => "BUY";

		private BuyCoinPos()
		{

		}
		public class BuyCoinPosConsensusFactory : ConsensusFactory
		{
			private BuyCoinPosConsensusFactory()
			{
			}
			public static BuyCoinPosConsensusFactory Instance { get; } = new BuyCoinPosConsensusFactory();

			public override BlockHeader CreateBlockHeader()
			{
				return new BuyCoinPosBlockHeader();
			}
			public override Block CreateBlock()
			{
				return new BuyCoinPosBlock(new BuyCoinPosBlockHeader());
			}
			protected override TransactionBuilder CreateTransactionBuilderCore(Network network)
			{
				var txBuilder = base.CreateTransactionBuilderCore(network);
				txBuilder.StandardTransactionPolicy.MinFee = Money.Coins(1m);
				return txBuilder;
			}
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public class AuxPow : IBitcoinSerializable
		{
			Transaction tx = new Transaction();

			public Transaction Transactions
			{
				get
				{
					return tx;
				}
				set
				{
					tx = value;
				}
			}

			uint nIndex = 0;

			public uint Index
			{
				get
				{
					return nIndex;
				}
				set
				{
					nIndex = value;
				}
			}

			uint256 hashBlock = new uint256();

			public uint256 HashBlock
			{
				get
				{
					return hashBlock;
				}
				set
				{
					hashBlock = value;
				}
			}

			List<uint256> vMerkelBranch = new List<uint256>();

			public List<uint256> MerkelBranch
			{
				get
				{
					return vMerkelBranch;
				}
				set
				{
					vMerkelBranch = value;
				}
			}

			List<uint256> vChainMerkleBranch = new List<uint256>();

			public List<uint256> ChainMerkleBranch
			{
				get
				{
					return vChainMerkleBranch;
				}
				set
				{
					vChainMerkleBranch = value;
				}
			}

			uint nChainIndex = 0;

			public uint ChainIndex
			{
				get
				{
					return nChainIndex;
				}
				set
				{
					nChainIndex = value;
				}
			}

			BlockHeader parentBlock = new BlockHeader();

			public BlockHeader ParentBlock
			{
				get
				{
					return parentBlock;
				}
				set
				{
					parentBlock = value;
				}
			}

			public void ReadWrite(BitcoinStream stream)
			{
				stream.ReadWrite(ref tx);
				stream.ReadWrite(ref hashBlock);
				stream.ReadWrite(ref vMerkelBranch);
				stream.ReadWrite(ref nIndex);
				stream.ReadWrite(ref vChainMerkleBranch);
				stream.ReadWrite(ref nChainIndex);
				stream.ReadWrite(ref parentBlock);
			}
		}

		public class BuyCoinPosBlock : Block
		{
			public BuyCoinPosBlock(BuyCoinPosBlockHeader header) : base(header)
			{

			}

			public override ConsensusFactory GetConsensusFactory()
			{
				return BuyCoinPosConsensusFactory.Instance;
			}
		}
		public class BuyCoinPosBlockHeader : BlockHeader
		{
			const int VERSION_AUXPOW = (1 << 8);

			AuxPow auxPow = new AuxPow();

			public AuxPow AuxPow
			{
				get
				{
					return auxPow;
				}
				set
				{
					auxPow = value;
				}
			}

			public override uint256 GetPoWHash()
			{
				var headerBytes = this.ToBytes();
				var h = NBitcoin.Crypto.SCrypt.ComputeDerivedKey(headerBytes, headerBytes, 1024, 1, 1, null, 32);
				return new uint256(h);
			}

			public override void ReadWrite(BitcoinStream stream)
			{
				base.ReadWrite(stream);
				if((Version & VERSION_AUXPOW) != 0)
				{
					if(!stream.Serializing)
					{
						stream.ReadWrite(ref auxPow);
					}
				}
			}
		}
#pragma warning restore CS0618 // Type or member is obsolete

		//Format visual studio
		//{({.*?}), (.*?)}
		//Tuple.Create(new byte[]$1, $2)
		//static Tuple<byte[], int>[] pnSeed6_main = null;
		//static Tuple<byte[], int>[] pnSeed6_test = null;
		// Not used in DOGE: https://github.com/dogecoin/dogecoin/blob/10a5e93a055ab5f239c5447a5fe05283af09e293/src/chainparams.cpp#L135
		

		static uint256 GetPoWHash(BlockHeader header)
		{
			var headerBytes = header.ToBytes();
			var h = NBitcoin.Crypto.SCrypt.ComputeDerivedKey(headerBytes, headerBytes, 1024, 1, 1, null, 32);
			return new uint256(h);
		}

		protected override NetworkBuilder CreateMainnet()
		{
			var builder = new NetworkBuilder();
			builder.SetConsensus(new Consensus()
			{
				SubsidyHalvingInterval = 100000,
				MajorityEnforceBlockUpgrade = 1500,
				MajorityRejectBlockOutdated = 1900,
				MajorityWindow = 2000,
				PowLimit = new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
				PowTargetTimespan = TimeSpan.FromSeconds(4 * 60 * 60),
				PowTargetSpacing = TimeSpan.FromSeconds(60),
				PowAllowMinDifficultyBlocks = false,
				CoinbaseMaturity = 90,
				//  Not set in reference client, assuming false
				PowNoRetargeting = false,
				//RuleChangeActivationThreshold = 6048,
				//MinerConfirmationWindow = 8064,
				ConsensusFactory = BuyCoinPosConsensusFactory.Instance,
				LitecoinWorkCalculation = true,
				SupportSegwit = false
			})
			.SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 26 })
			.SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 55 })
			.SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 191 })
			.SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x04, 0x68, 0xB2, 0x1E })
			.SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x04, 0x58, 0xAD, 0xE4 })
			.SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("bcp"))
			.SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("bcp"))
			.SetMagic(0x40fbad4)
			.SetPort(9037)
			.SetRPCPort(9038)
			.SetName("bcp-main")
			.AddAlias("bcp-mainnet")
			.AddAlias("buycoinpos-mainnet")
			.AddAlias("buycoinpos-main")
			.AddDNSSeeds(new[]
			{
				new DNSSeedData("buy-coin.me", "seed.buy-coin.me"),
				new DNSSeedData("185.2.101.142", "seed2.buy-coin.me")
			})
			.AddSeeds(new NetworkAddress[0])
			.SetGenesis("010000000000000000000000000000000000000000000000000000000000000000000000b653fea44c6559808953888761df2b2c50444581355f73f43eb6a806060e16c8cd4ae959ffff0f1e385a01000101000000cd4ae959010000000000000000000000000000000000000000000000000000000000000000ffffffff3600012a32323031372c204e6f76656d6265722031382c20427579436f696e20504f53204261736564206d616b6520666f722073776170ffffffff01000000000000000000000000000040fba7d442010000010000003f7041842f704f124c68cc237f5fcaca5015955dbabf1b80d42376d5620700004cf71f519136cbd99898e8f45b70fe200352d135dafa2f46dca150f2bb34ecdb00a3e959ffff0f1e46666ef8010100000000a3e959010000000000000000000000000000000000000000000000000000000000000000ffffffff49510400a3e95908fabe6d6d0000000001ae451000000010ffffffff00007f5693245ff00000000000000001010000000000000040000000010000000d2f");
			return builder;
		}

		protected override NetworkBuilder CreateTestnet()
		{
			var builder = new NetworkBuilder();
			builder.SetConsensus(new Consensus()
			{
				SubsidyHalvingInterval = 100000,
				MajorityEnforceBlockUpgrade = 501,
				MajorityRejectBlockOutdated = 750,
				MajorityWindow = 1000,
				PowLimit = new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
				// pre-post-digishield https://github.com/dogecoin/dogecoin/blob/10a5e93a055ab5f239c5447a5fe05283af09e293/src/chainparams.cpp#L45
				PowTargetTimespan = TimeSpan.FromSeconds(60),
				PowTargetSpacing = TimeSpan.FromSeconds(60),
				PowAllowMinDifficultyBlocks = true,
				CoinbaseMaturity = 240,
				//  Not set in reference client, assuming false
				PowNoRetargeting = false,
				//RuleChangeActivationThreshold = 6048,
				//MinerConfirmationWindow = 8064,
				LitecoinWorkCalculation = true,
				ConsensusFactory = BuyCoinPosConsensusFactory.Instance,
				SupportSegwit = false
			})
			.SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 113 })
			.SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 196 })
			.SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 241 })
			.SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x04, 0x35, 0x87, 0xCF })
			.SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x04, 0x35, 0x83, 0x94 })
			.SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("tbcp"))
			.SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("tbcp"))
			.SetMagic(0xdcb7c1fc)
			.SetPort(44556)
			.SetRPCPort(22555)
		   .SetName("bcp-test")
		   .AddAlias("bcp-testnet")
		   .AddAlias("buycoinpos-test")
		   .AddAlias("buycoinpos-testnet")
		   .AddDNSSeeds(new[]
		   {
				new DNSSeedData("jrn.me.uk", "testseed.jrn.me.uk")
		   })
		   .AddSeeds(new NetworkAddress[0])
		   .SetGenesis("010000000000000000000000000000000000000000000000000000000000000000000000696ad20e2dd4365c7459b4a4a5af743d5e92c6da3229e6532cd605f6533f2a5bb9a7f052f0ff0f1ef7390f000101000000010000000000000000000000000000000000000000000000000000000000000000ffffffff1004ffff001d0104084e696e746f6e646fffffffff010058850c020000004341040184710fa689ad5023690c80f3a49c8f13f8d45b8c857fbcbc8bc4a8e4d3eb4b10f4d4604fa08dce601aaf0f470216fe1b51850b4acf21b179c45070ac7b03a9ac00000000");
			return builder;
		}

		protected override NetworkBuilder CreateRegtest()
		{
			var builder = new NetworkBuilder();
			builder.SetConsensus(new Consensus()
			{
				SubsidyHalvingInterval = 150,
				MajorityEnforceBlockUpgrade = 750,
				MajorityRejectBlockOutdated = 950,
				MajorityWindow = 1000,
				PowLimit = new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
				PowTargetTimespan = TimeSpan.FromSeconds(4 * 60 * 60),
				PowTargetSpacing = TimeSpan.FromSeconds(60),
				PowAllowMinDifficultyBlocks = false,
				CoinbaseMaturity = 60,
				//  Not set in reference client, assuming false
				PowNoRetargeting = false,
				//RuleChangeActivationThreshold = 6048,
				//MinerConfirmationWindow = 8064,
				LitecoinWorkCalculation = true,
				ConsensusFactory = BuyCoinPosConsensusFactory.Instance,
				SupportSegwit = false
			})
			.SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 113 })
			.SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 196 })
			.SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 241 })
			.SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x04, 0x35, 0x87, 0xCF })
			.SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x04, 0x35, 0x83, 0x94 })
			.SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("tbcp"))
			.SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("tbcp"))
			.SetMagic(0xdab5bffa)
			.SetPort(18444)
			.SetRPCPort(44555) // by default this is assigned dynamically, adding port I got for testing
			.SetName("bcp-reg")
			.AddAlias("bcp-regtest")
			.AddAlias("buycoinpos-regtest")
			.AddAlias("buycoinpos-reg")
			.AddDNSSeeds(new DNSSeedData[0])
			.AddSeeds(new NetworkAddress[0])
			.SetGenesis("010000000000000000000000000000000000000000000000000000000000000000000000696ad20e2dd4365c7459b4a4a5af743d5e92c6da3229e6532cd605f6533f2a5bdae5494dffff7f20020000000101000000010000000000000000000000000000000000000000000000000000000000000000ffffffff1004ffff001d0104084e696e746f6e646fffffffff010058850c020000004341040184710fa689ad5023690c80f3a49c8f13f8d45b8c857fbcbc8bc4a8e4d3eb4b10f4d4604fa08dce601aaf0f470216fe1b51850b4acf21b179c45070ac7b03a9ac00000000");
			return builder;
		}

		protected override void PostInit()
		{
			RegisterDefaultCookiePath("BuyCoinPos");
		}

	}
}
