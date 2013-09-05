using System.Collections.Generic;

namespace Catalogue.Gemini.Vocabs
{
    public static class TopicCategories
    {
        /// <summary>
        /// The topic category codes (and descriptions) required by UK Gemini.
        /// </summary>
        public static readonly Dictionary<string, string> Values = new Dictionary<string, string>
            {
                { "biota", "flora and/or fauna in natural environment. Examples: wildlife, vegetation, biological sciences, ecology, wilderness, sealife, wetlands, habitat" },
                { "boundaries", "legal land descriptions. Examples: political and administrative boundaries" },
                { "climatologyMeteorologyAtmosphere", "processes and phenomena of the atmosphere. Examples: cloud cover, weather, climate, atmospheric conditions, climate change, precipitation" },
                { "economy", "economic activities, conditions and employment. Examples: production, labour, revenue, commerce, industry, tourism and ecotourism, forestry, fisheries, commercial or subsistence hunting, exploration and exploitation of resources such as minerals, oil and gas" },
                { "elevation", "height above or below sea level. Examples: altitude, bathymetry, digital elevation models, slope, derived products" },
                { "environment", "environmental resources, protection and conservation. Examples: environmental pollution, waste storage and treatment, environmental impact assessment, monitoring environmental risk, nature reserves, landscape" },
                { "farming", "rearing of animals and/or cultivation of plants. Examples: agriculture, irrigation, aquaculture, plantations, herding, pests and diseases affecting crops and livestock" },
                { "geoscientificInformation", "information pertaining to earth sciences. Examples: geophysical features and processes, geology, minerals, sciences dealing with the composition, structure and origin of the earth s rocks, risks of earthquakes, volcanic activity, landslides, gravity information, soils, permafrost, hydrogeology, erosion" },
                { "health", "health, health services, human ecology, and safety. Examples: disease and illness, factors affecting health, hygiene, substance abuse, mental and physical health, health services" },
                { "imageryBaseMapsEarthCover", "base maps. Examples: land cover, topographic maps, imagery, unclassified images, annotations" },
                { "inlandWaters", "inland water features, drainage systems and their characteristics. Examples: rivers and glaciers, salt lakes, water utilization plans, dams, currents, floods, water quality, hydrographic charts" },
                { "intelligenceMilitary", "military bases, structures, activities. Examples: barracks, training grounds, military transportation, information collection" },
                { "location", "positional information and services. Examples: addresses, geodetic networks, control points, postal zones and services, place names" },
                { "oceans", "features and characteristics of salt water bodies (excluding inland waters). Examples: tides, tidal waves, coastal information, reefs" },
                { "planningCadastre", "information used for appropriate actions for future use of the land. Examples: land use maps, zoning maps, cadastral surveys, land ownership" },
                { "society", "characteristics of society and cultures. Examples: settlements, anthropology, archaeology, education, traditional beliefs, manners and customs, demographic data, recreational areas and activities, social impact assessments, crime and justice, census information" },
                { "structure", "man-made construction. Examples: buildings, museums, churches, factories, housing, monuments, shops, towers" },
                { "transportation", "means and aids for conveying persons and/or goods. Examples: roads, airports/airstrips, shipping routes, tunnels, nautical charts, vehicle or vessel location, aeronautical charts, railways" },
                { "utilitiesCommunication", "energy, water and waste systems and communications infrastructure and services. Examples: hydroelectricity, geothermal, solar and nuclear sources of energy, water purification and distribution, sewage collection and disposal, electricity and gas distribution, data communication, telecommunication, radio, communication networks" },
            };
    }
}
