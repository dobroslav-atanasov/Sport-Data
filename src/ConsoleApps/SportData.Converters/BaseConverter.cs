namespace SportData.Converters;

using System.Text;

using Dasync.Collections;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using SportData.Common.Extensions;
using SportData.Data.Entities.Crawlers;
using SportData.Services.Data.CrawlerStorageDb.Interfaces;
using SportData.Services.Interfaces;

public abstract class BaseConverter
{
    private readonly ICrawlersService crawlersService;
    private readonly ILogsService logsService;
    private readonly IGroupsService groupsService;
    private readonly IZipService zipService;

    public BaseConverter(ILogger<BaseConverter> logger, ICrawlersService crawlersService, ILogsService logsService, IGroupsService groupsService, IZipService zipService)
    {
        this.Logger = logger;
        this.crawlersService = crawlersService;
        this.logsService = logsService;
        this.groupsService = groupsService;
        this.zipService = zipService;
    }

    protected ILogger<BaseConverter> Logger { get; }

    protected abstract Task ProcessGroupAsync(Group group);

    public async Task ConvertAsync(string crawlerName)
    {
        this.Logger.LogInformation($"Converter: {crawlerName} start.");

        try
        {
            var crawlerId = await this.crawlersService.GetCrawlerIdAsync(crawlerName);
            var identifiers = await this.logsService.GetLogIdentifiersAsync(crawlerId);

            //            identifiers = new List<Guid>
            //            {
            //                Guid.Parse("d377c5e3-d373-4257-9c5f-242d7fdc54e0"),
            //Guid.Parse("d0f1ae64-f402-4959-b9b4-f2db17b98982"),
            //Guid.Parse("b16859cd-00a6-489c-af85-1c70019f3827"),
            //Guid.Parse("1a732ea8-3af6-46e5-9a43-b3379eebdd79"),
            //Guid.Parse("b235c16c-64a9-42a3-8d1d-255fbd71d937"),
            //Guid.Parse("87aff6c1-0709-47c9-9c28-71f573bc1be2"),
            //Guid.Parse("da844996-fe26-43a4-b863-d5b790d883ea"),
            //Guid.Parse("0d654cba-d467-4a7b-bf87-3fbbd32c3c2c"),
            //Guid.Parse("b7e8cad8-0f17-4498-b28e-0aa58cd75b3f"),
            //Guid.Parse("54780da2-25be-4a28-a338-21c1fa6d04a2"),
            //Guid.Parse("2f89f542-078f-4aca-afe7-cd7a0475a99c"),
            //Guid.Parse("38507e61-1592-40f2-b705-ddfe8e0861a7"),
            //Guid.Parse("8ed81cb1-5096-4e7c-80c8-503bb357840a"),
            //Guid.Parse("9e55be0f-4a30-4945-9cdf-4f37d34ddc2f"),
            //Guid.Parse("2a5fa6bf-6e6b-427c-b043-1bcdabe53337"),
            //Guid.Parse("a77bd86e-7454-4967-807b-8f49e84861ef"),
            //Guid.Parse("ab404b40-b6b5-4d58-a381-500cc3a9f255"),
            //Guid.Parse("91b12cc7-2a5d-479e-a096-61b2533759a8"),
            //Guid.Parse("3deb8131-31ad-4263-8384-faa75991972b"),
            //Guid.Parse("63d4f096-b340-42d1-9faf-0ef041b88a0f"),
            //Guid.Parse("698ac712-e1df-448b-94ea-1fe0c8c417e0"),
            //Guid.Parse("fefc95d2-f3e5-495f-8d43-113098877d08"),
            //Guid.Parse("52ba21bb-98bf-46f2-9b12-362d77666a2b"),
            //Guid.Parse("1cba36a5-b74b-4f52-9e8e-4e24385f0c8d"),
            //Guid.Parse("76e9d4fa-4de5-469b-9acc-3fd50487ee31"),
            //Guid.Parse("41b45ab5-c06b-408e-b98f-53b7ccc4e8fd"),
            //Guid.Parse("75a9e028-faf2-4aba-a2c6-d0de262a61ad"),
            //Guid.Parse("f098806f-5ee2-43af-a74d-fdc06201caa3"),
            //Guid.Parse("a6f18669-a251-4aa0-bf42-dc4d8f2fb27d"),
            //Guid.Parse("3db67a2c-6dd5-42f8-b735-092174bc2522"),
            //Guid.Parse("22f5448e-0e29-4a7c-8367-3fc842eb0e3b"),
            //Guid.Parse("7e909ce9-fb97-4edc-aeeb-6b65a87ce5ee"),
            //Guid.Parse("2b4b332b-0c83-4e6c-ab2b-811bfa3499f3"),
            //Guid.Parse("f68edbc7-4c3a-45d6-be00-7d8b0c98035a"),
            //Guid.Parse("51706ba6-a6b5-45d0-853e-61a02a1283ce"),
            //Guid.Parse("d89ceb69-448c-4fbf-9113-32eaf1552669"),
            //Guid.Parse("dccc3e59-eef7-4b3c-b6cf-cc238c2bea25"),
            //Guid.Parse("06f7f79f-8ead-4777-999e-c5d693f1effb"),
            //Guid.Parse("c219eb2c-dd4e-4dc2-8e1e-199ddf00332b"),
            //Guid.Parse("88280a5e-436e-4292-ba62-691bdd1fa440"),
            //Guid.Parse("a89722f5-78fc-4420-95fb-9e408db095b4"),
            //Guid.Parse("4b809de0-d8d5-4fa7-98dc-adc3e07473e9"),
            //Guid.Parse("3752ecba-54a4-4994-afff-a526bd05e8cd"),
            //Guid.Parse("580dea4a-8363-486c-810c-4e2b410f3291"),
            //Guid.Parse("f6a6a6b2-2310-421e-bed2-7974983b6e2d"),
            //Guid.Parse("31c3cf90-e3d2-41c5-bfa2-a70c9c141a97"),
            //Guid.Parse("6300a852-3f07-4a41-af07-8efb694e00be"),
            //Guid.Parse("a113ad40-1013-4560-bb7e-2e99b582dafc"),
            //Guid.Parse("0fc68007-5b55-4976-ac6b-6d00ce3ddc87"),
            //Guid.Parse("52cc285c-dc4e-4778-84ea-64bf0c822a5f"),
            //Guid.Parse("5a60b191-f2e3-45e5-8228-21dffdb967c4"),
            //Guid.Parse("656fe244-2a90-4fb3-8276-5ef5fcb017b5"),
            //Guid.Parse("6beb516b-a7e0-4414-98f3-8ab32b47131c"),
            //Guid.Parse("c8e51528-d205-4ce1-97a6-d97f69a034e7"),
            //Guid.Parse("c9df578b-e63a-4bab-9ab0-851010dd1aa3"),
            //Guid.Parse("944d005d-0ecf-444f-95d1-aa627dd267a6"),
            //Guid.Parse("b7266d27-c909-4489-8d50-c1aba3037134"),
            //Guid.Parse("a9785486-6a21-45cb-a74b-198e775e7f69"),
            //Guid.Parse("2f0d04d1-25f2-48f9-a915-9a4e65455dcf"),
            //Guid.Parse("7a6f96f0-15df-4b7d-b6c0-171cfd99ecd8"),
            //Guid.Parse("589a660e-2380-41e4-96fe-a1f0065bbab1"),
            //Guid.Parse("afb9cf81-58c9-4ba5-a2e3-3da3ca9c115e"),
            //Guid.Parse("e3cb4497-f40f-41ba-abf9-5dc477fa5972"),
            //Guid.Parse("adaf5eac-2901-4afa-87be-5a279a913617"),
            //Guid.Parse("133df44a-36a0-4745-a464-8d53b83fe709"),
            //Guid.Parse("02d5530f-10f1-4e82-b5dc-1c21ee5d6972"),
            //Guid.Parse("2dcbb602-9b66-4e08-99da-bf0a838873ea"),
            //Guid.Parse("a48b3869-547a-41f6-b79e-395b9ba6372e"),
            //Guid.Parse("3110cc87-6bd2-40e8-be2c-1cbc4f4861b2"),
            //Guid.Parse("58e391c1-9431-4cdc-a380-33cda5e8bbb2"),
            //Guid.Parse("1364f987-c55f-4951-a2d2-1afec812bcbe"),
            //Guid.Parse("de3472fb-d35a-469c-85be-26f4f68c7d0e"),
            //Guid.Parse("f2d7a42b-800a-459c-8fd0-04e65bf94965"),
            //Guid.Parse("ed4b6204-227b-48c8-aa46-d8aa58df73d5"),
            //Guid.Parse("4571e254-f257-4abb-b6c7-233072fcfa2d"),
            //Guid.Parse("7ed9dda6-5902-40d3-9194-a89ff4c159b5"),
            //Guid.Parse("965899d1-6f24-42ec-9a16-2c658ccbc9f5"),
            //Guid.Parse("16429e6a-0b47-4392-a27e-fbc0e1e5ac1f"),
            //Guid.Parse("4c239955-1985-4800-a00a-612157c040a0"),
            //Guid.Parse("30e208ba-da25-4ed3-95a7-cc0fd933d1fc"),
            //Guid.Parse("ecd90c90-7fa6-40ed-a5e9-1f11f52cabea"),
            //Guid.Parse("fbfb732e-0834-48f2-9256-b7ae435cf638"),
            //Guid.Parse("aea976e9-bff6-42ce-87d6-531647c63559"),
            //Guid.Parse("5862f41a-84b9-46b1-b995-f4b28e7453ba"),
            //Guid.Parse("bce6fb26-04a0-456c-9be5-99632cb27b32"),
            //Guid.Parse("76862e1c-0662-44f9-8c9f-bad5c09995ca"),
            //Guid.Parse("75098a3c-3738-47c5-b8b2-6c7139d0455b"),
            //Guid.Parse("98e52b1c-7e97-4fb7-acaa-69e73fe96ac3"),
            //Guid.Parse("ef9c2a3d-b703-4e9c-9b53-51a482d8d77d"),
            //Guid.Parse("dddea1a4-0926-4521-9fd1-bb016e56efa7"),
            //Guid.Parse("aa79bc03-7f14-49f5-bf8d-77dde44200d6"),
            //Guid.Parse("fb323e3d-fc12-43db-90b7-98a89ea01d1b"),
            //Guid.Parse("af31b041-4e83-44b2-ab01-498ba8c1e035"),
            //Guid.Parse("ab109226-b8a5-4007-aa1f-ab6249e75399"),
            //Guid.Parse("d056e67f-3117-4754-b388-de7f0029d761"),
            //Guid.Parse("31902284-a7f5-4dfb-acb4-d0ef130bc43e"),
            //Guid.Parse("d603144b-e522-4cef-93ad-a207eebeef5e"),
            //Guid.Parse("062f4bf2-8477-41fe-b327-71dacdd0e557"),
            //Guid.Parse("519a2823-77ce-492a-a870-04c7a109853f"),
            //Guid.Parse("04ad8872-b756-4227-aa1d-700979209d1d"),
            //Guid.Parse("b4cebeb1-4695-44f7-8df6-4c19d42ece30"),
            //Guid.Parse("37e41485-8259-4139-9b00-9cfa184211b0"),
            //Guid.Parse("1cb244ff-6f30-45c2-bba9-862617e4bfe9"),
            //Guid.Parse("3f25b205-e7db-45b3-8283-b8f321614e39"),
            //Guid.Parse("ac59622b-610c-401b-a956-d10d09732c44"),
            //Guid.Parse("f4970d20-2892-48e8-96bd-b8a021475c99"),
            //Guid.Parse("beefb88e-0dbd-48cc-9b28-d64d9dc0bd4a"),
            //Guid.Parse("4c5c2d1e-73aa-4dad-adbd-df4c1f0ee185"),
            //Guid.Parse("d2b3c66f-8f00-415b-8232-6ba2e8bcfe64"),
            //Guid.Parse("e46f81d2-cb8f-486c-b40a-491a9131c7f3"),
            //Guid.Parse("bef4da3a-bc64-4827-a871-5772f61acece"),
            //Guid.Parse("4b424d3b-4fd7-422d-9106-346146442f7f"),
            //Guid.Parse("ade85c65-dd26-4c33-9eab-15eee7cbde9c"),
            //Guid.Parse("19377211-28db-421f-89ab-e11b2ec630a7"),
            //Guid.Parse("942bca19-322c-4614-b1be-74340d74c972"),
            //Guid.Parse("225cde2f-8777-4f1f-9a80-93d53ff6a885"),
            //Guid.Parse("14eafe6f-80cf-4a66-b2b6-59d8a3257e17"),
            //Guid.Parse("8e014284-5f73-479c-b103-7804e0654025"),
            //Guid.Parse("e42a2445-b2ba-484a-a0f7-74c725fb8eac"),
            //Guid.Parse("fbbbbc09-d76d-4173-9a95-0030c0e4b1a1"),
            //Guid.Parse("c440468c-f826-470b-8782-af238b3ab0e0"),
            //Guid.Parse("05c760fc-f218-4a55-8726-70efd351015e"),
            //Guid.Parse("b79c3b9c-3ef0-449c-821a-9092b3c460eb"),
            //Guid.Parse("dd80bc8e-91f1-438e-8cb4-29c0cdb2e647"),
            //Guid.Parse("aa5ff27b-1e11-4887-80e9-a2baf0fa177c"),
            //Guid.Parse("b0b138b7-71c8-48f7-842b-369e3a553185"),
            //Guid.Parse("4a71caef-e72d-47b6-8082-4b42b1c017de"),
            //Guid.Parse("2aee997c-9e1f-4e15-b451-f7372b65a1c4"),
            //Guid.Parse("54831419-1343-4bdb-abab-6efafa91f0ee"),
            //Guid.Parse("12542ce7-aa02-4b6e-8c59-23d81b54fb19"),
            //Guid.Parse("c4aa6f7a-7c32-4d81-8f44-f6957982e8a3"),
            //Guid.Parse("a4fef6fe-7fd1-43fe-9a0f-a1807f27f38a"),
            //Guid.Parse("41292ccb-9556-4d63-a8a1-ccba58133aaa"),
            //Guid.Parse("dee0f667-5903-46e9-adad-9698753ab97d"),
            //Guid.Parse("1caa7849-cbe7-4843-9a48-8b8d2a580037"),
            //Guid.Parse("665e63b4-3458-4df8-854e-68f9e66ace62"),
            //Guid.Parse("a5b8b6b0-0d89-414c-bca8-ad37013d8858"),
            //Guid.Parse("f8c51d33-9931-4026-b8db-b5d39d49868c"),
            //Guid.Parse("3fc648c5-f0b3-43b9-b49a-df9dafccb447"),
            //Guid.Parse("370ec882-0bef-4a01-89a6-9babe5b44633"),
            //Guid.Parse("4f29fb9f-050b-432d-a893-bce7dd1e1c01"),
            //Guid.Parse("aa0f3a49-4a75-4ebd-b20c-f1794a84c5cb"),
            //Guid.Parse("e2c564b7-40e4-4a0d-938a-32d8d22858d9"),
            //Guid.Parse("d72df7ff-5172-4be6-80f5-09b7b2dec45e"),
            //Guid.Parse("1959ce88-8127-4167-82d9-a01e3fecc340"),
            //Guid.Parse("b575cf0c-10f8-4927-958a-23841779154b"),
            //Guid.Parse("9f66eff2-3567-42cb-8b9c-9924c4e363de"),
            //Guid.Parse("978ac831-6959-47ce-842d-6b6a19e02eb5"),
            //Guid.Parse("b701823a-3290-45df-b08e-431ff6d9b319"),
            //Guid.Parse("1ce56351-afec-4541-8119-b9e0cb5d1638"),
            //Guid.Parse("9c75ed8d-7236-44a6-9931-2b6aadf5e2d6"),
            //Guid.Parse("301f8f86-a296-43b1-ad2b-22df7ecfa671"),
            //Guid.Parse("e1db8c59-1442-4e18-8a24-7e54699e3bd0"),
            //Guid.Parse("0346a8d1-dbba-4145-bd1f-41fd508c2710"),
            //Guid.Parse("8e417e79-137a-4285-9873-8e0e1041cda1"),
            //Guid.Parse("dbd496a1-f5c9-4d53-b221-c7f301238267"),
            //Guid.Parse("ef5f43c7-db94-4848-b5aa-132cb47867ff"),
            //Guid.Parse("acb22eba-fddf-40af-86cd-cbc8ae1173ec"),
            //Guid.Parse("4a6a31d0-1f9c-4055-a582-1a1490764be0"),
            //Guid.Parse("e16b0476-477b-4001-bd09-e2588bbd0499"),
            //Guid.Parse("848c14cf-389d-4b96-83c8-1618fc9f2401"),
            //Guid.Parse("ad4d991c-c518-469c-9ddb-8f706face220"),
            //Guid.Parse("b67db05a-812c-4224-b26d-89e914a850a3"),
            //Guid.Parse("617971f1-f24a-4ac1-b947-7d35acd87c5f"),
            //Guid.Parse("1e7ad9a3-7962-4492-9773-583ac779aadf"),
            //Guid.Parse("dfb10cfa-f33a-4df5-8b5d-0d3169c74f23"),
            //Guid.Parse("5f30ebc7-54e6-459a-849a-e32f6c997511"),
            //Guid.Parse("870fc0f2-6547-4de0-85a3-e9dce49a905f"),
            //Guid.Parse("f6501c4d-6c07-40a3-80a3-d8f8bfa36c33"),
            //Guid.Parse("88866725-dc75-4c9b-b623-302a2a4b4a59"),
            //Guid.Parse("61ca2625-15f9-42d0-ad6e-5a4613e75c01"),
            //Guid.Parse("06bbcd9f-2395-43af-addb-899f9f569d97"),
            //Guid.Parse("04f1b090-aae6-4cec-88e4-63ce891e9842"),
            //Guid.Parse("cf266483-6a24-4585-8ecb-8d5e849057b2"),
            //Guid.Parse("80fca7bf-6dba-40ca-aace-bf17de3bbd54"),
            //Guid.Parse("83266846-966b-480f-a8bc-8aaa20f63942"),
            //Guid.Parse("58472bc8-e817-469c-9a1a-508d3a83bd67"),
            //Guid.Parse("c37aef05-286c-4024-9547-e5b340bb0038"),
            //Guid.Parse("b44e9ec1-9708-4f6b-b9ce-3f33d2a1cdaf"),
            //Guid.Parse("084bde5f-112c-4736-8504-8cd4193a7b10"),
            //Guid.Parse("329bca8e-9ce4-48dc-9239-824c33914a45"),
            //Guid.Parse("8aa69193-8b7c-4def-b0e9-b7a1a8b4e61f"),
            //Guid.Parse("11b59037-eba8-4e13-bf42-f576acd5b999"),
            //Guid.Parse("a0fe4cf4-6c6a-45dd-9624-8515dd930763"),
            //Guid.Parse("9000661d-85c3-43d7-b418-3ca905695190"),
            //Guid.Parse("3651fe4a-d8cd-4f88-ab5b-f337f7491fe6"),
            //Guid.Parse("c4025702-4eba-42ea-a4c9-530f6a988bcb"),
            //Guid.Parse("21a41302-1959-49f7-b3c1-64afbbc17282"),
            //Guid.Parse("ea4f5340-688d-4849-a836-fab4eec6c99d"),
            //Guid.Parse("1c5b0805-37c0-46ae-a619-90fa2a707bb9"),
            //Guid.Parse("548e956b-041f-4ebb-8f2f-ce2016994b85"),
            //Guid.Parse("52046d6d-f9f0-46d0-ad0a-4895c11f4bad"),
            //Guid.Parse("df2d4afc-3453-4eac-8e8a-30899e3ee221"),
            //Guid.Parse("fd707fcf-1fdb-4ba6-b1e1-3b9e23d4c2ae"),
            //Guid.Parse("28383fc2-f0a9-4e86-bee0-1aead7706109"),
            //Guid.Parse("71e8fd02-0a46-491b-8cec-4014222fad62"),
            //Guid.Parse("68795148-5567-4a70-98d6-6affef6e07ab"),
            //Guid.Parse("53a3649d-2764-472b-bc18-dafa87302f40"),
            //Guid.Parse("a68f74fc-21c0-45eb-b72e-5bdc2631c29c"),
            //Guid.Parse("33f59c76-d42b-4209-8ad7-04e9a06dd9d0"),
            //Guid.Parse("30a67e8b-ac71-4fe0-8709-63af2dd84bb8"),
            //Guid.Parse("0db89394-7cc9-4fe4-b1fe-e833e9a614bf"),
            //Guid.Parse("fcdf6b66-d284-4a7a-86c9-ca6390a7afaa"),
            //Guid.Parse("5c1700fd-047e-4f30-aaac-742271b51871"),
            //Guid.Parse("cf73a3d4-cd07-4fba-8b0d-75032fc69ae7"),
            //Guid.Parse("845912d3-0873-4806-9c34-cd65b028e84f"),
            //Guid.Parse("4ecee6d4-62c1-4e35-9844-c6e35bb5cf17"),
            //Guid.Parse("91150e3a-c0ce-4e8b-812a-dfe4654f87bc"),
            //Guid.Parse("a90e0bbc-a81f-4d9f-bcc5-86999bdce9d7"),
            //Guid.Parse("397ea03a-42f5-4a7c-ba1c-30c42650faec"),
            //Guid.Parse("cda9fb7e-7c86-4b3c-8fd3-18910e19aa91"),
            //Guid.Parse("2c96c96a-142b-43ff-9aa1-284b45f30ec9"),
            //Guid.Parse("1eb924f6-6e68-4e26-8fca-003d6164691b"),
            //Guid.Parse("b88cd8d3-049f-4804-b8c5-0b8c97c749bc"),
            //Guid.Parse("131e329c-7388-4872-8329-7150271c6870"),
            //Guid.Parse("ecde76f2-a509-4e1e-9cc8-ee08c8d7b499"),
            //Guid.Parse("3307dc94-196a-49f1-82f8-70317760eb83"),
            //Guid.Parse("79237217-4a5a-4a1c-a41c-cb81fea3611e"),
            //Guid.Parse("91c6e4f4-fbf2-4bc1-aa64-2104138a5186"),
            //Guid.Parse("721c3dc0-46a3-48a3-82aa-7586cc9f283b"),
            //Guid.Parse("317ee633-b560-425b-8218-2113c3d05471"),
            //Guid.Parse("d25c5e68-0ab5-413c-94cf-468461d26ec8"),
            //Guid.Parse("27856b08-adcd-48b1-b827-b47ac3391039"),
            //            };

            await identifiers.ParallelForEachAsync(async identifier =>
            {
                try
                {
                    var group = await this.groupsService.GetGroupAsync(identifier);
                    var zipModels = this.zipService.UnzipGroup(group.Content);
                    foreach (var document in group.Documents)
                    {
                        var zipModel = zipModels.First(z => z.Name == document.Name);
                        document.Content = zipModel.Content;
                    }

                    await this.ProcessGroupAsync(group);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, $"Group was not process: {identifier};");
                }
            }, maxDegreeOfParallelism: 1);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"Failed to process documents from converter: {crawlerName};");
        }

        this.Logger.LogInformation($"Converter: {crawlerName} end.");
    }

    protected HtmlDocument CreateHtmlDocument(Document document)
    {
        var encoding = Encoding.GetEncoding(document.Encoding);
        var html = encoding.GetString(document.Content).Decode();
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);

        return htmlDocument;
    }
}