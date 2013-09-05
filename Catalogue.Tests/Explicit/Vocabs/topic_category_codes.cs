using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace Catalogue.Tests.Explicit.Vocabs
{
    class topic_category_codes
    {
        [Explicit]
        [Test]
        public void extract_topic_category_codes_from_xml()
        {
            var p = XElement.Parse(xml);
            var q = from e in p.Element("CodeListDictionary").Elements("codeEntry")
                    let f = e.Element("CodeDefinition")
                    let description = f.Element("description").Value
                    let identifier = f.Element("identifier").Value
                    orderby identifier
                    select string.Format("                {{ \"{0}\", \"{1}\" }},", identifier, description);

            File.WriteAllLines(@"c:\work\file.txt", q);
        }

        // from http://www.isotc211.org/2005/resources/Codelist/gmxCodelists.xml
        string xml = @"<codelistItem>
<CodeListDictionary id=""MD_TopicCategoryCode"">
<description>
high-level geographic data thematic classification to assist in the grouping and search of available geographic data sets. Can be used to group keywords as well. Listed examples are not exhaustive.
</description>
<identifier codeSpace=""ISOTC211/19115"">MD_TopicCategoryCode</identifier>
<codeEntry>
<CodeDefinition id=""MD_TopicCategoryCode_farming"">
<description>
rearing of animals and/or cultivation of plants. Examples: agriculture, irrigation, aquaculture, plantations, herding, pests and diseases affecting crops and livestock
</description>
<identifier codeSpace=""ISOTC211/19115"">farming</identifier>
</CodeDefinition>
</codeEntry>
<codeEntry>
<CodeDefinition id=""MD_TopicCategoryCode_biota"">
<description>
flora and/or fauna in natural environment. Examples: wildlife, vegetation, biological sciences, ecology, wilderness, sealife, wetlands, habitat
</description>
<identifier codeSpace=""ISOTC211/19115"">biota</identifier>
</CodeDefinition>
</codeEntry>
<codeEntry>
<CodeDefinition id=""MD_TopicCategoryCode_boundaries"">
<description>
legal land descriptions. Examples: political and administrative boundaries
</description>
<identifier codeSpace=""ISOTC211/19115"">boundaries</identifier>
</CodeDefinition>
</codeEntry>
<codeEntry>
<CodeDefinition id=""MD_TopicCategoryCode_climatologyMeteorologyAtmosphere"">
<description>
processes and phenomena of the atmosphere. Examples: cloud cover, weather, climate, atmospheric conditions, climate change, precipitation
</description>
<identifier codeSpace=""ISOTC211/19115"">climatologyMeteorologyAtmosphere</identifier>
</CodeDefinition>
</codeEntry>
<codeEntry>
<CodeDefinition id=""MD_TopicCategoryCode_economy"">
<description>
economic activities, conditions and employment. Examples: production, labour, revenue, commerce, industry, tourism and ecotourism, forestry, fisheries, commercial or subsistence hunting, exploration and exploitation of resources such as minerals, oil and gas
</description>
<identifier codeSpace=""ISOTC211/19115"">economy</identifier>
</CodeDefinition>
</codeEntry>
<codeEntry>
<CodeDefinition id=""MD_TopicCategoryCode_elevation"">
<description>
height above or below sea level. Examples: altitude, bathymetry, digital elevation models, slope, derived products
</description>
<identifier codeSpace=""ISOTC211/19115"">elevation</identifier>
</CodeDefinition>
</codeEntry>
<codeEntry>
<CodeDefinition id=""MD_TopicCategoryCode_environment"">
<description>
environmental resources, protection and conservation. Examples: environmental pollution, waste storage and treatment, environmental impact assessment, monitoring environmental risk, nature reserves, landscape
</description>
<identifier codeSpace=""ISOTC211/19115"">environment</identifier>
</CodeDefinition>
</codeEntry>
<codeEntry>
<CodeDefinition id=""MD_TopicCategoryCode_geoscientificInformation"">
<description>
information pertaining to earth sciences. Examples: geophysical features and processes, geology, minerals, sciences dealing with the composition, structure and origin of the earth s rocks, risks of earthquakes, volcanic activity, landslides, gravity information, soils, permafrost, hydrogeology, erosion
</description>
<identifier codeSpace=""ISOTC211/19115"">geoscientificInformation</identifier>
</CodeDefinition>
</codeEntry>
<codeEntry>
<CodeDefinition id=""MD_TopicCategoryCode_health"">
<description>
health, health services, human ecology, and safety. Examples: disease and illness, factors affecting health, hygiene, substance abuse, mental and physical health, health services
</description>
<identifier codeSpace=""ISOTC211/19115"">health</identifier>
</CodeDefinition>
</codeEntry>
<codeEntry>
<CodeDefinition id=""MD_TopicCategoryCode_imageryBaseMapsEarthCover"">
<description>
base maps. Examples: land cover, topographic maps, imagery, unclassified images, annotations
</description>
<identifier codeSpace=""ISOTC211/19115"">imageryBaseMapsEarthCover</identifier>
</CodeDefinition>
</codeEntry>
<codeEntry>
<CodeDefinition id=""MD_TopicCategoryCode_intelligenceMilitary"">
<description>
military bases, structures, activities. Examples: barracks, training grounds, military transportation, information collection
</description>
<identifier codeSpace=""ISOTC211/19115"">intelligenceMilitary</identifier>
</CodeDefinition>
</codeEntry>
<codeEntry>
<CodeDefinition id=""MD_TopicCategoryCode_inlandWaters"">
<description>
inland water features, drainage systems and their characteristics. Examples: rivers and glaciers, salt lakes, water utilization plans, dams, currents, floods, water quality, hydrographic charts
</description>
<identifier codeSpace=""ISOTC211/19115"">inlandWaters</identifier>
</CodeDefinition>
</codeEntry>
<codeEntry>
<CodeDefinition id=""MD_TopicCategoryCode_location"">
<description>
positional information and services. Examples: addresses, geodetic networks, control points, postal zones and services, place names
</description>
<identifier codeSpace=""ISOTC211/19115"">location</identifier>
</CodeDefinition>
</codeEntry>
<codeEntry>
<CodeDefinition id=""MD_TopicCategoryCode_oceans"">
<description>
features and characteristics of salt water bodies (excluding inland waters). Examples: tides, tidal waves, coastal information, reefs
</description>
<identifier codeSpace=""ISOTC211/19115"">oceans</identifier>
</CodeDefinition>
</codeEntry>
<codeEntry>
<CodeDefinition id=""MD_TopicCategoryCode_planningCadastre"">
<description>
information used for appropriate actions for future use of the land. Examples: land use maps, zoning maps, cadastral surveys, land ownership
</description>
<identifier codeSpace=""ISOTC211/19115"">planningCadastre</identifier>
</CodeDefinition>
</codeEntry>
<codeEntry>
<CodeDefinition id=""MD_TopicCategoryCode_society"">
<description>
characteristics of society and cultures. Examples: settlements, anthropology, archaeology, education, traditional beliefs, manners and customs, demographic data, recreational areas and activities, social impact assessments, crime and justice, census information
</description>
<identifier codeSpace=""ISOTC211/19115"">society</identifier>
</CodeDefinition>
</codeEntry>
<codeEntry>
<CodeDefinition id=""MD_TopicCategoryCode_structure"">
<description>
man-made construction. Examples: buildings, museums, churches, factories, housing, monuments, shops, towers
</description>
<identifier codeSpace=""ISOTC211/19115"">structure</identifier>
</CodeDefinition>
</codeEntry>
<codeEntry>
<CodeDefinition id=""MD_TopicCategoryCode_transportation"">
<description>
means and aids for conveying persons and/or goods. Examples: roads, airports/airstrips, shipping routes, tunnels, nautical charts, vehicle or vessel location, aeronautical charts, railways
</description>
<identifier codeSpace=""ISOTC211/19115"">transportation</identifier>
</CodeDefinition>
</codeEntry>
<codeEntry>
<CodeDefinition id=""MD_TopicCategoryCode_utilitiesCommunication"">
<description>
energy, water and waste systems and communications infrastructure and services. Examples: hydroelectricity, geothermal, solar and nuclear sources of energy, water purification and distribution, sewage collection and disposal, electricity and gas distribution, data communication, telecommunication, radio, communication networks
</description>
<identifier codeSpace=""ISOTC211/19115"">utilitiesCommunication</identifier>
</CodeDefinition>
</codeEntry>
</CodeListDictionary>
</codelistItem>
        ";
    }
}
