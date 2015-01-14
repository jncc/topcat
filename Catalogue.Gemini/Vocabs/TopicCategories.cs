using System.Collections.Generic;

namespace Catalogue.Gemini.Vocabs
{
    public static class TopicCategories
    {
        /// <summary>
        /// The topic category codes (and descriptions) required by UK Gemini.
        /// </summary>
        public static readonly List<TopicCategory> Values = new List<TopicCategory>
            {
                new TopicCategory { Name = "biota", Relevant = true, Description = "flora and/or fauna in natural environment. Examples: wildlife, vegetation, biological sciences, ecology, wilderness, sealife, wetlands, habitat" },
                new TopicCategory { Name = "boundaries", Relevant = true, Description = "legal land descriptions. Examples: political and administrative boundaries" },
                new TopicCategory { Name = "climatologyMeteorologyAtmosphere", Relevant = false, Description = "processes and phenomena of the atmosphere. Examples: cloud cover, weather, climate, atmospheric conditions, climate change, precipitation" },
                new TopicCategory { Name = "economy", Relevant = true, Description = "economic activities, conditions and employment. Examples: production, labour, revenue, commerce, industry, tourism and ecotourism, forestry, fisheries, commercial or subsistence hunting, exploration and exploitation of resources such as minerals, oil and gas" },
                new TopicCategory { Name = "elevation", Relevant = true, Description = "height above or below sea level. Examples: altitude, bathymetry, digital elevation models, slope, derived products" },
                new TopicCategory { Name = "environment", Relevant = true, Description = "environmental resources, protection and conservation. Examples: environmental pollution, waste storage and treatment, environmental impact assessment, monitoring environmental risk, nature reserves, landscape" },
                new TopicCategory { Name = "farming", Relevant = true, Description = "rearing of animals and/or cultivation of plants. Examples: agriculture, irrigation, aquaculture, plantations, herding, pests and diseases affecting crops and livestock" },
                new TopicCategory { Name = "geoscientificInformation", Relevant = true, Description = "information pertaining to earth sciences. Examples: geophysical features and processes, geology, minerals, sciences dealing with the composition, structure and origin of the earth s rocks, risks of earthquakes, volcanic activity, landslides, gravity information, soils, permafrost, hydrogeology, erosion" },
                new TopicCategory { Name = "health", Relevant = false, Description = "health, health services, human ecology, and safety. Examples: disease and illness, factors affecting health, hygiene, substance abuse, mental and physical health, health services" },
                new TopicCategory { Name = "imageryBaseMapsEarthCover", Relevant = false, Description = "base maps. Examples: land cover, topographic maps, imagery, unclassified images, annotations" },
                new TopicCategory { Name = "inlandWaters", Relevant = true, Description = "inland water features, drainage systems and their characteristics. Examples: rivers and glaciers, salt lakes, water utilization plans, dams, currents, floods, water quality, hydrographic charts" },
                new TopicCategory { Name = "intelligenceMilitary", Relevant = false, Description = "military bases, structures, activities. Examples: barracks, training grounds, military transportation, information collection" },
                new TopicCategory { Name = "location", Relevant = true, Description = "positional information and services. Examples: addresses, geodetic networks, control points, postal zones and services, place names" },
                new TopicCategory { Name = "oceans", Relevant = true, Description = "features and characteristics of salt water bodies (excluding inland waters). Examples: tides, tidal waves, coastal information, reefs" },
                new TopicCategory { Name = "planningCadastre", Relevant = false, Description = "information used for appropriate actions for future use of the land. Examples: land use maps, zoning maps, cadastral surveys, land ownership" },
                new TopicCategory { Name = "society", Relevant = false, Description = "characteristics of society and cultures. Examples: settlements, anthropology, archaeology, education, traditional beliefs, manners and customs, demographic data, recreational areas and activities, social impact assessments, crime and justice, census information" },
                new TopicCategory { Name = "structure", Relevant = false, Description = "man-made construction. Examples: buildings, museums, churches, factories, housing, monuments, shops, towers" },
                new TopicCategory { Name = "transportation", Relevant = false, Description = "means and aids for conveying persons and/or goods. Examples: roads, airports/airstrips, shipping routes, tunnels, nautical charts, vehicle or vessel location, aeronautical charts, railways" },
                new TopicCategory { Name = "utilitiesCommunication", Relevant = false, Description = "energy, water and waste systems and communications infrastructure and services. Examples: hydroelectricity, geothermal, solar and nuclear sources of energy, water purification and distribution, sewage collection and disposal, electricity and gas distribution, data communication, telecommunication, radio, communication networks" },
            };
    }

    public class TopicCategory
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Relevant { get; set; } // a quick way to remove the irrelevant ones
    }
}
